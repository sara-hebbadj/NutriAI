using System.Collections.ObjectModel;
using System.ComponentModel;
using NutriAI.Models;
using NutriAI.Services;
using NutriAI.Services.Recommendation;

namespace NutriAI.ViewModels;

public class HomeViewModel : INotifyPropertyChanged
{
    private readonly IRecipeService _recipeService;
    private readonly IRecommendationService _recommender;

    public HomeViewModel(
        IRecipeService recipeService,
        IRecommendationService recommender)
    {
        _recipeService = recipeService;
        _recommender = recommender;

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
    // FILTER + AI RANKING PIPELINE
    // ========================
    private async void ApplyAllFilters()
    {
        FilteredRecipes.Clear();

        // 1️⃣ Search + filter relevance
        var filtered = AllRecipes
            .Select(r => new
            {
                Recipe = r,
                FilterScore = CalculateScore(r)
            })
            .Where(x => x.FilterScore > 0)
            .Select(x => x.Recipe)
            .ToList();

        // 2️⃣ AI ranking (time decay + context)
        var ranked = await _recommender.RankAsync(filtered);

        // 3️⃣ Update UI
        foreach (var r in ranked)
            FilteredRecipes.Add(r);
    }

    // ========================
    // FILTER RELEVANCE SCORE
    // ========================
    private int CalculateScore(Recipe r)
    {
        int score = 0;

        // Search relevance
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

        // Meal type
        if (!string.IsNullOrWhiteSpace(SelectedMealType) &&
            r.MealType.Equals(SelectedMealType,
                StringComparison.OrdinalIgnoreCase))
            score += 2;

        // Diet
        if (!string.IsNullOrWhiteSpace(SelectedDiet) &&
            r.Diet.Equals(SelectedDiet,
                StringComparison.OrdinalIgnoreCase))
            score += 2;

        // Cuisine
        if (!string.IsNullOrWhiteSpace(SelectedCuisine) &&
            r.Cuisine.Equals(SelectedCuisine,
                StringComparison.OrdinalIgnoreCase))
            score += 1;

        // Default (no filters/search)
        if (string.IsNullOrWhiteSpace(_searchQuery) &&
            SelectedMealType == null &&
            SelectedDiet == null &&
            SelectedCuisine == null)
            score = 1;

        return score;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
