using System.Collections.ObjectModel;
using NutriAI.Models;
using NutriAI.Services;

namespace NutriAI.ViewModels;

public class SavedRecipesViewModel
{
    private readonly IRecipeService _recipeService = ServiceLocator.RecipeService;

    public ObservableCollection<Recipe> SavedRecipes => _recipeService.GetSavedRecipes();
}
