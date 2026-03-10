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
        //  Always load latest prefs (includes allergies)
        var preferences = await _preferencesStore.LoadAsync()
                          ?? new UserPreferences();

        var interactions = await _interactionService.GetAllAsync();
        var context = ContextHelper.GetCurrentMealContext();

        var map = interactions.ToDictionary(x => x.RecipeId, x => x);

        var recipeList = candidates.ToList();

        // =====================================================
        // 1️ Compute global normalization statistics (ONCE)
        // =====================================================
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
                var interaction = map.TryGetValue(r.Id, out var i)
                    ? i
                    : new UserRecipeInteraction { RecipeId = r.Id };

                // =====================================================
                // 2️ Preprocessing: Feature Vector Construction
                // =====================================================
                var features = RecipeFeatureNormalizer.FromRecipe(r, stats);

                // =====================================================
                // 3️ Score using enriched feature representation
                // =====================================================
                var score = RecipeScorer.Score(
                    r,
                    features,
                    interaction,
                    context,
                    preferences);

                // =====================================================
                // 4️ Explanation
                // =====================================================
                r.RecommendationReasons =
                    RecommendationExplainer.Explain(
                        r,
                        interaction,
                        context,
                        preferences);

                return (Recipe: r, Score: score);
            })
            // HARD FILTER: remove illegal recipes (halal/allergies)
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Select(x => x.Recipe)
            .ToList();
    }
}
