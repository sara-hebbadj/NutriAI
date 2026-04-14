// Authorship note:
// This file was written by the author for a simple view-model.
// External help was limited to standard .NET MAUI / MVVM syntax examples and minor boilerplate support from copilot.
// The page purpose, bound fields, and its role in the application were decided by the author.

using System.Collections.ObjectModel;
using System.ComponentModel;
using NutriAI.Models;
using NutriAI.Services;
using NutriAI.Services.Recommendation;
using NutriAI.Services.Storage;

namespace NutriAI.ViewModels;

public class HomeViewModel : INotifyPropertyChanged
{
    private readonly IRecipeService _recipeService;
    private readonly IRecommendationService _recommender;
    private readonly IUserPreferencesStore _preferencesStore;

    public HomeViewModel(
        IRecipeService recipeService,
        IRecommendationService recommender,
        IUserPreferencesStore preferencesStore)
    {
        _recipeService = recipeService;
        _recommender = recommender;
        _preferencesStore = preferencesStore;

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
        try
        {
            FilteredRecipes.Clear();

            // 1️ Search + filter relevance
            var filtered = AllRecipes
                .Where(r => r != null)
                .Select(r => new
                {
                    Recipe = r,
                    FilterScore = CalculateScore(r)
                })
                .Where(x => x.FilterScore > 0)
                .Select(x => x.Recipe)
                .ToList();

            // 2️ AI ranking (time decay + context)
            var preferences = await _preferencesStore.LoadAsync()
                               ?? new UserPreferences();

            var ranked = await _recommender.RankAsync(filtered, preferences);

            // 3️ Update UI
            foreach (var r in ranked.Where(x => x != null))
                FilteredRecipes.Add(r);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HOME FILTER ERROR] {ex.Message}");
        }
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

            if ((r.Ingredients ?? new List<string>()).Any(i =>
                !string.IsNullOrWhiteSpace(i) &&
                i.Contains(q, StringComparison.OrdinalIgnoreCase)))
                score += 2;
        }

        // Meal type
        if (!string.IsNullOrWhiteSpace(SelectedMealType))
        {
            var selectedMeal = SelectedMealType.ToLowerInvariant();
            var recipeMeal = r.MealType?.ToLowerInvariant() ?? "";

            var mealMatches =
                recipeMeal == selectedMeal ||
                (recipeMeal == "meal" &&
                 (selectedMeal == "lunch" || selectedMeal == "dinner"));

            if (mealMatches)
                score += 2;
        }

        // Diet
        if (!string.IsNullOrWhiteSpace(SelectedDiet))
        {
            var selectedDiet = SelectedDiet.ToLowerInvariant();
            var recipeDiet = r.Diet?.ToLowerInvariant() ?? "";

            if (recipeDiet == selectedDiet)
                score += 2;
        }

        // Cuisine
        if (!string.IsNullOrWhiteSpace(SelectedCuisine))
        {
            var selectedCuisine = SelectedCuisine.ToLowerInvariant();
            var recipeCuisine = r.Cuisine?.ToLowerInvariant() ?? "";

            if (recipeCuisine == selectedCuisine)
                score += 1;
        }

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