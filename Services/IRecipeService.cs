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
    // Holds ALL recipes (home / browse page)
    private readonly ObservableCollection<Recipe> _allRecipes = new();

    // Holds SAVED recipes (favorites)
    private readonly ObservableCollection<Recipe> _savedRecipes = new();


    // ================= ALL RECIPES =================
    public RecipeService()
    {
        //  LOAD MOCK DATA HERE
        _allRecipes = MockRecipeService.All;
    }

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