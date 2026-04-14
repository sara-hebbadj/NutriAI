// Authorship note:
// This file was written by the author for a simple NutriAI view-model.
// External help was limited to standard .NET MAUI / MVVM syntax examples and minor boilerplate support from copilot.
// The page purpose, bound fields, and its role in the application were decided by the author.

using System.ComponentModel;
using System.Windows.Input;
using NutriAI.Models;
using NutriAI.Services;
using NutriAI.Services.Interactions;
using NutriAI.Services.Storage;
using NutriAI.Services.Recommendation;

namespace NutriAI.ViewModels;

public class RecipeDetailsViewModel : INotifyPropertyChanged
{
    private readonly IRecipeService _recipeService;
    private readonly IUserInteractionService _interactionService;

    private Recipe _recipe;
    public Recipe Recipe
    {
        get => _recipe;
        private set
        {
            _recipe = value;
            OnPropertyChanged(nameof(Recipe));
        }
    }

    private UserRecipeInteraction? _interaction;

    public ICommand ToggleSaveCommand { get; }
    public ICommand CookCommand { get; }

    private bool _isSaved;
    public bool IsSaved
    {
        get => _isSaved;
        private set
        {
            _isSaved = value;
            OnPropertyChanged(nameof(IsSaved));
            OnPropertyChanged(nameof(SaveButtonText));
        }
    }

    public string SaveButtonText =>
        IsSaved ? "Unsave Recipe" : "Save Recipe";

    // =========================
    // CONTEXTUAL HINT (OPTION A)
    // =========================
    public string? MealContextHint
    {
        get
        {
            if (_interaction == null ||
                string.IsNullOrWhiteSpace(_interaction.LastUsedMealType))
                return null;

            return $"You usually cook this for {_interaction.LastUsedMealType}";
        }
    }

    // =========================
    // CONSTRUCTOR
    // =========================
    private readonly IUserPreferencesStore _preferencesStore;

    public RecipeDetailsViewModel(
        IRecipeService recipeService,
        IUserInteractionService interactionService,
        IUserPreferencesStore preferencesStore)
    {
        _recipeService = recipeService;
        _interactionService = interactionService;
        _preferencesStore = preferencesStore;

        ToggleSaveCommand = new Command(ToggleSave);
        CookCommand = new Command(CookRecipe);
    }


    // =========================
    // NAVIGATION ENTRY
    // =========================
    public async Task SetRecipeAsync(Recipe recipe)
    {
        if (recipe == null)
            return;

        Recipe = recipe;

        // 1️ Record view
        await _interactionService.RecordViewAsync(recipe.Id);

        // 2️ Load interaction
        _interaction = await _interactionService.GetAsync(recipe.Id);
        OnPropertyChanged(nameof(MealContextHint));

        // 3️ Saved state
        IsSaved = _recipeService.IsRecipeSaved(recipe);

        // 4️ Load preferences
        var preferences = await _preferencesStore.LoadAsync()
            ?? new UserPreferences();

        // 5️ Generate explanation
        var context = ContextHelper.GetCurrentMealContext();

        Recipe.RecommendationReasons =
            RecommendationExplainer.Explain(
                Recipe,
                _interaction ?? new UserRecipeInteraction { RecipeId = recipe.Id },
                context,
                preferences);

        OnPropertyChanged(nameof(Recipe));

        // 6️ Load full recipe details
        await LoadDetailsAsync();
    }


    // =========================
    // LOAD FULL DETAILS
    // =========================
    public async Task LoadDetailsAsync()
    {
        if (Recipe == null)
            return;

        if (_recipeService is ApiRecipeService api)
        {
            var fullRecipe = await api.GetRecipeDetailsAsync(Recipe.Id);
            if (fullRecipe == null)
                return;

            Recipe.Calories = fullRecipe.Calories;
            Recipe.ProteinGrams = fullRecipe.ProteinGrams;
            Recipe.CarbsGrams = fullRecipe.CarbsGrams;
            Recipe.FatGrams = fullRecipe.FatGrams;
            Recipe.CookingTimeMinutes = fullRecipe.CookingTimeMinutes;
            Recipe.Ingredients = fullRecipe.Ingredients ?? new();
            Recipe.Instructions = fullRecipe.Instructions;

            OnPropertyChanged(nameof(Recipe));
        }
    }

    // =========================
    // SAVE / UNSAVE
    // =========================
    private async void ToggleSave()
    {
        if (IsSaved)
        {
            _recipeService.UnsaveRecipe(Recipe);
        }
        else
        {
            _recipeService.SaveRecipe(Recipe);
            await _interactionService.RecordSaveAsync(Recipe.Id);
        }

        IsSaved = !IsSaved;
    }

    // =========================
    // COOK WITH MEAL SELECTION
    // =========================
    private async void CookRecipe()
    {
        if (Recipe == null)
            return;

        var action = await Shell.Current.DisplayActionSheet(
            "What meal was this?",
            "Cancel",
            null,
            "Breakfast",
            "Lunch",
            "Dinner",
            "Snack");

        if (action == null || action == "Cancel")
            return;

        var mealType = action.ToLowerInvariant();

        await _interactionService.RecordCookAsync(
            Recipe.Id,
            mealType);

        // Reload interaction so hint updates
        _interaction = await _interactionService.GetAsync(Recipe.Id);
        OnPropertyChanged(nameof(MealContextHint));

        await Shell.Current.DisplayAlert(
            "Logged",
            $"Marked as cooked for {mealType} ✅",
            "OK");
    }

    // =========================
    // INotifyPropertyChanged
    // =========================
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

}
