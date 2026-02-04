using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NutriAI.Models;

namespace NutriAI.Services.Interactions;

public interface IUserInteractionService
{
    Task RecordViewAsync(string recipeId);
    Task RecordSaveAsync(string recipeId);
    Task RecordCookAsync(string recipeId, string mealType);

    Task<UserRecipeInteraction?> GetAsync(string recipeId);
    Task<IReadOnlyList<UserRecipeInteraction>> GetAllAsync();

}
