// Authorship note:
// This interface was designed by the author to support NutriAI interaction tracking.
// The view/save/cook interaction model and the decision to expose async methods for retrieving
// interaction records were defined by the author.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NutriAI.Models;

namespace NutriAI.Services.Interactions;

// This interface defines the interaction-tracking operations used by the app.
// It keeps the recommender logic separate from the database implementation.
public interface IUserInteractionService
{
    // Record that the user opened a recipe.
    Task RecordViewAsync(string recipeId);

    // Record that the user saved a recipe for later.
    Task RecordSaveAsync(string recipeId);

    // Record that the user cooked a recipe, including the meal context.
    Task RecordCookAsync(string recipeId, string mealType);

    // Get the stored interaction history for one recipe.
    Task<UserRecipeInteraction?> GetAsync(string recipeId);

    // Get all stored interaction rows for recommendation and analysis.
    Task<IReadOnlyList<UserRecipeInteraction>> GetAllAsync();

}
