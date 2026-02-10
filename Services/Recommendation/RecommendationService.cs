using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NutriAI.Models;
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
        // ✅ Always load latest prefs (includes allergies)
        var preferences = await _preferencesStore.LoadAsync()
                          ?? new UserPreferences();

        var interactions = await _interactionService.GetAllAsync();
        var context = ContextHelper.GetCurrentMealContext();

        var map = interactions.ToDictionary(x => x.RecipeId, x => x);

        return candidates
            .Select(r =>
            {
                var interaction = map.TryGetValue(r.Id, out var i)
                    ? i
                    : new UserRecipeInteraction { RecipeId = r.Id };

                var score = RecipeScorer.Score(
                    r,
                    interaction,
                    context,
                    preferences);

                // EXPLANATION 
                r.RecommendationReasons =
                    RecommendationExplainer.Explain(
                        r,
                        interaction,
                        context,
                        preferences);

                return (Recipe: r, Score: score);
            })
            //  HARD FILTER: remove illegal recipes (halal/allergies)
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Select(x => x.Recipe)
            .ToList();
    }
}
