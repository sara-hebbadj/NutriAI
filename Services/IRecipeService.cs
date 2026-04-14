// Authorship note:
// This interface and the simple in-memory recipe service were written by the author.
// Microsoft documentation was used as reference for ObservableCollection and LINQ usage.
// The decision to separate recipe access behind IRecipeService and to keep
// all-recipes and saved-recipes collections distinct in NutriAI was made by the author.

using NutriAI.Models;
using System.Collections.ObjectModel;

namespace NutriAI.Services;

public interface IRecipeService
{
    ObservableCollection<Recipe> GetAllRecipes();
    ObservableCollection<Recipe> GetSavedRecipes();

    void SaveRecipe(Recipe recipe);
    void UnsaveRecipe(Recipe recipe);
    bool IsRecipeSaved(Recipe recipe);
}

public class RecipeService : IRecipeService
{
    // Holds all recipes shown on the home / browse view.
    private readonly ObservableCollection<Recipe> _allRecipes = new();

    // Holds the user's saved recipes.
    private readonly ObservableCollection<Recipe> _savedRecipes = new();

    // ========================
    // ALL RECIPES
    // ========================
    public ObservableCollection<Recipe> GetAllRecipes()
        => _allRecipes;

    // ================= SAVED RECIPES =================
    public ObservableCollection<Recipe> GetSavedRecipes()
        => _savedRecipes;

    public void SaveRecipe(Recipe recipe)
    {
        // Only save the recipe if it is not already in the saved list.
        if (!_savedRecipes.Any(r => r.Id == recipe.Id))
            _savedRecipes.Add(recipe);
    }

    public void UnsaveRecipe(Recipe recipe)
    {
        var existing = _savedRecipes.FirstOrDefault(r => r.Id == recipe.Id);

        if (existing != null)
            _savedRecipes.Remove(existing);
    }

    public bool IsRecipeSaved(Recipe recipe)
        => _savedRecipes.Any(r => r.Id == recipe.Id);
}