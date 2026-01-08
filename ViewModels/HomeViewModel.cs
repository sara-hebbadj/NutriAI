using System.Collections.ObjectModel;
using System.ComponentModel;
using NutriAI.Models;
using NutriAI.Services;

namespace NutriAI.ViewModels;

public class HomeViewModel : INotifyPropertyChanged
{
    private readonly IRecipeService _recipeService =
        ServiceLocator.RecipeService;

    // ALL recipes (source of truth)
    public ObservableCollection<Recipe> AllRecipes { get; }

    // What the UI actually shows
    public ObservableCollection<Recipe> FilteredRecipes { get; } = new();

    // Optional filters (future-ready)
    public string? SelectedMealType { get; set; }
    public string? SelectedDiet { get; set; }
    public string? SelectedCuisine { get; set; }

    public HomeViewModel()
    {
        AllRecipes = _recipeService.GetAllRecipes();

        // Default: show all recipes
        foreach (var r in AllRecipes)
            FilteredRecipes.Add(r);
    }

    // ========================
    // SEARCH
    // ========================
    public void ApplySearch(string? query)
    {
        FilteredRecipes.Clear();

        // If no search, show all (Home feed behavior)
        if (string.IsNullOrWhiteSpace(query))
        {
            foreach (var r in AllRecipes)
                FilteredRecipes.Add(r);
            return;
        }

        string q = query.ToLowerInvariant();

        var results = AllRecipes.Where(r =>
            (!string.IsNullOrWhiteSpace(r.Title) &&
                r.Title.ToLowerInvariant().Contains(q)) ||

            (r.Ingredients != null &&
                r.Ingredients.Any(i =>
                    !string.IsNullOrWhiteSpace(i) &&
                    i.ToLowerInvariant().Contains(q))) ||

            (!string.IsNullOrWhiteSpace(r.MealType) &&
                r.MealType.ToLowerInvariant().Contains(q)) ||

            (!string.IsNullOrWhiteSpace(r.Diet) &&
                r.Diet.ToLowerInvariant().Contains(q)) ||

            (!string.IsNullOrWhiteSpace(r.Cuisine) &&
                r.Cuisine.ToLowerInvariant().Contains(q))
        );

        foreach (var r in results)
            FilteredRecipes.Add(r);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
