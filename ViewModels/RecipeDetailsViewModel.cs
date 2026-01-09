using System.ComponentModel;
using System.Windows.Input;
using NutriAI.Models;
using NutriAI.Services;

namespace NutriAI.ViewModels;

public class RecipeDetailsViewModel : INotifyPropertyChanged
{
    private readonly IRecipeService _recipeService;

    public Recipe Recipe { get; }

    public ICommand ToggleSaveCommand { get; }

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

    public RecipeDetailsViewModel(Recipe recipe)
    {
        Recipe = recipe;
        _recipeService = ServiceLocator.RecipeService;

        IsSaved = _recipeService.IsRecipeSaved(recipe);

        ToggleSaveCommand = new Command(ToggleSave);
    }

    private void ToggleSave()
    {
        if (IsSaved)
            _recipeService.UnsaveRecipe(Recipe);
        else
            _recipeService.SaveRecipe(Recipe);

        IsSaved = !IsSaved;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    public async Task LoadDetailsAsync()
    {
        if (_recipeService is ApiRecipeService api)
        {
            var fullRecipe = await api.GetRecipeDetailsAsync(Recipe.Id);

            Recipe.Calories = fullRecipe.Calories;
            Recipe.ProteinGrams = fullRecipe.ProteinGrams;
            Recipe.CarbsGrams = fullRecipe.CarbsGrams;
            Recipe.FatGrams = fullRecipe.FatGrams;
            Recipe.CookingTimeMinutes = fullRecipe.CookingTimeMinutes;
            Recipe.Ingredients = fullRecipe.Ingredients;
            Recipe.Instructions = fullRecipe.Instructions;

            OnPropertyChanged(nameof(Recipe));
        }
    }

}