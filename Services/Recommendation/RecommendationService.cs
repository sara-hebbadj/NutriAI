// Authorship note:
// Microsoft documentation was used in this file for LINQ methods such as Select, Where,
// OrderByDescending, ToList, ToDictionary, Min, and Max.
// Copilot was used to help draft and refine the ranking pipeline structure.
// The NutriAI specific pipeline: loading preferences and interactions, computing normalized features,
// calling the scorer, filtering invalid recipes, ranking results, and attaching explanations 
// was designed, adapted, and tested by the author.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NutriAI.Models;
using NutriAI.Preprocessing;
using NutriAI.Services.Interactions;
using NutriAI.Services.Storage;

namespace NutriAI.Services.Recommendation;

public class RecommendationService : IRecommendationService
{
    private readonly IUserInteractionService _interactionService;
    private readonly IUserPreferencesStore _preferencesStore;

    public RecommendationService(
        IUserInteractionService interactionService,
        IUserPreferencesStore preferencesStore)
    {
        _interactionService = interactionService;
        _preferencesStore = preferencesStore;
    }

    public async Task<IReadOnlyList<Recipe>> RankAsync(
    IEnumerable<Recipe> candidates,
    UserPreferences _)
    {
        // Always load the latest saved preferences so the ranking
        // uses the user's current dietary rules, allergies, and goals.
        var preferences = await _preferencesStore.LoadAsync()
                          ?? new UserPreferences();
        // Load all recorded user interactions and detect the current meal context.
        var interactions = await _interactionService.GetAllAsync();
        var context = ContextHelper.GetCurrentMealContext();

        return Rank(candidates, preferences, interactions, context);
    }

    // Testability change (October 2026, portfolio version):
    // the ranking steps below were moved unchanged out of RankAsync into this pure method,
    // so unit tests can rank recipes with a fixed meal context and no SQLite or file storage.
    public static IReadOnlyList<Recipe> Rank(
        IEnumerable<Recipe> candidates,
        UserPreferences preferences,
        IReadOnlyList<UserRecipeInteraction> interactions,
        MealContext context)
    {
        // Convert the interaction list into a lookup by recipe id
        // so existing interaction data can be found quickly during ranking.
        var map = interactions.ToDictionary(x => x.RecipeId, x => x);

        // Materialize the candidate sequence once so it can be reused
        // for normalization and later ranking.
        var recipeList = candidates.ToList();

        // Compute global nutrition ranges once for the whole candidate set.
        // These ranges are used to normalize recipe features consistently.
        var stats = new NutritionStats
        {
            MinCalories = recipeList.Min(r => r.Calories),
            MaxCalories = recipeList.Max(r => r.Calories),

            MinProtein = recipeList.Min(r => r.ProteinGrams),
            MaxProtein = recipeList.Max(r => r.ProteinGrams),

            MinCarbs = recipeList.Min(r => r.CarbsGrams),
            MaxCarbs = recipeList.Max(r => r.CarbsGrams),

            MinFat = recipeList.Min(r => r.FatGrams),
            MaxFat = recipeList.Max(r => r.FatGrams),

            MinTime = recipeList.Min(r => r.CookingTimeMinutes),
            MaxTime = recipeList.Max(r => r.CookingTimeMinutes)
        };

        return recipeList
            .Select(r =>
            {
                // Use the stored interaction record if it exists,
                // otherwise start with an empty interaction profile for this recipe.
                var interaction = map.TryGetValue(r.Id, out var i)
                    ? i
                    : new UserRecipeInteraction { RecipeId = r.Id };

                // Convert the raw recipe into normalized feature values
                // so scoring can work with comparable numeric inputs.
                var features = RecipeFeatureNormalizer.FromRecipe(r, stats);

                // Compute the final recommendation score using filtering,
                // behavioural signals, recency, context, and preference weights.
                var score = RecipeScorer.Score(
                    r,
                    features,
                    interaction,
                    context,
                    preferences);

                // Generate simple recommendation explanation for the UI
                // using the same main inputs used in the ranking process.
                r.RecommendationReasons =
                    RecommendationExplainer.Explain(
                        r,
                        interaction,
                        context,
                        preferences);

                return (Recipe: r, Score: score);
            })
            // HARD FILTER: Remove recipes that scored zero because they failed hard constraints.
            .Where(x => x.Score > 0)
            // Sort the remaining valid recipes from highest score to lowest.
            .OrderByDescending(x => x.Score)
            .Select(x => x.Recipe)
            .ToList();
    }
}
