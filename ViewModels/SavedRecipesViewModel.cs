using System.Collections.ObjectModel;
using NutriAI.Models;
using NutriAI.Services;

namespace NutriAI.ViewModels;

public class SavedRecipesViewModel
{
    private readonly IRecipeService _recipeService;

    public SavedRecipesViewModel(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    public ObservableCollection<Recipe> SavedRecipes => _recipeService.GetSavedRecipes();
}
