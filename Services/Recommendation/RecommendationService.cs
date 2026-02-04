using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NutriAI.Models;
using NutriAI.Services.Interactions;

namespace NutriAI.Services.Recommendation;

public class RecommendationService : IRecommendationService
{
    private readonly IUserInteractionService _interactionService;

    public RecommendationService(IUserInteractionService interactionService)
    {
        _interactionService = interactionService;
    }

    public async Task<IReadOnlyList<Recipe>> RankAsync(IEnumerable<Recipe> candidates)
    {
        var interactions = await _interactionService.GetAllAsync();
        var context = ContextHelper.GetCurrentMealContext();

        // Map for fast lookup
        var map = interactions.ToDictionary(x => x.RecipeId, x => x);

        return candidates
            .Select(r =>
            {
                var interaction = map.TryGetValue(r.Id, out var i)
                    ? i
                    : new UserRecipeInteraction { RecipeId = r.Id };

                var score = RecipeScorer.Score(r, interaction, context);
                return (Recipe: r, Score: score);
            })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Recipe)
            .ToList();
    }
}

