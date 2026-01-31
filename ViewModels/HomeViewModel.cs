using System.Collections.ObjectModel;
using System.ComponentModel;
using NutriAI.Models;
using NutriAI.Services;

namespace NutriAI.ViewModels;

public class HomeViewModel : INotifyPropertyChanged
{
    private readonly IRecipeService _recipeService;

    public HomeViewModel(IRecipeService recipeService)
    {
        _recipeService = recipeService;
        AllRecipes = _recipeService.GetAllRecipes();
    }

    // ========================
    // DATA SOURCES
    // ========================
    public ObservableCollection<Recipe> AllRecipes { get; }
    public ObservableCollection<Recipe> FilteredRecipes { get; } = new();

    // ========================
    // FILTER STATE
    // ========================
    public string? SelectedMealType { get; set; }
    public string? SelectedDiet { get; set; }
    public string? SelectedCuisine { get; set; }

    private string? _searchQuery;




    // ========================
    // INITIAL LOAD
    // ========================
    public async Task InitializeAsync()
    {
        if (_recipeService is ApiRecipeService api)
            await api.LoadRecipesAsync();

        ApplyAllFilters();
    }

    // ========================
    // SEARCH
    // ========================
    public void ApplySearch(string? query)
    {
        _searchQuery = query;
        ApplyAllFilters();
    }

    // ========================
    // FILTER API
    // ========================
    public void ApplyFilters()
    {
        ApplyAllFilters();
    }

    public void ClearFilters()
    {
        SelectedMealType = null;
        SelectedDiet = null;
        SelectedCuisine = null;
        ApplyAllFilters();
    }

    // ========================
    // SCORING-BASED PIPELINE
    // ========================
    private void ApplyAllFilters()
    {
        FilteredRecipes.Clear();

        var ranked = AllRecipes
            .Select(r => new
            {
                Recipe = r,
                Score = CalculateScore(r)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Select(x => x.Recipe);

        foreach (var r in ranked)
            FilteredRecipes.Add(r);
    }

    // ========================
    // SCORING FUNCTION
    // ========================
    private int CalculateScore(Recipe r)
    {
        int score = 0;

        // 🔍 Search relevance
        if (!string.IsNullOrWhiteSpace(_searchQuery))
        {
            var q = _searchQuery.ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(r.Title) &&
                r.Title.Contains(q, StringComparison.OrdinalIgnoreCase))
                score += 3;

            if (r.Ingredients.Any(i =>
                i.Contains(q, StringComparison.OrdinalIgnoreCase)))
                score += 2;
        }

        // 🍽 Meal type
        if (!string.IsNullOrWhiteSpace(SelectedMealType) &&
            r.MealType.Equals(SelectedMealType,
                StringComparison.OrdinalIgnoreCase))
        {
            score += 2;
        }

        // 🥗 Diet
        if (!string.IsNullOrWhiteSpace(SelectedDiet) &&
            r.Diet.Equals(SelectedDiet,
                StringComparison.OrdinalIgnoreCase))
        {
            score += 2;
        }

        // 🌍 Cuisine
        if (!string.IsNullOrWhiteSpace(SelectedCuisine) &&
            r.Cuisine.Equals(SelectedCuisine,
                StringComparison.OrdinalIgnoreCase))
        {
            score += 1;
        }

        // Default: show everything when no filters/search
        if (string.IsNullOrWhiteSpace(_searchQuery) &&
            SelectedMealType == null &&
            SelectedDiet == null &&
            SelectedCuisine == null)
        {
            score = 1;
        }

        return score;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
