using System.ComponentModel;
using System.Windows.Input;
using NutriAI.Models;
using NutriAI.Services;

namespace NutriAI.ViewModels;

public class RecipeDetailsViewModel : INotifyPropertyChanged
{
    private readonly IRecipeService _recipeService;

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

    // ✅ DI constructor (ONLY constructor)
    public RecipeDetailsViewModel(IRecipeService recipeService)
    {
        _recipeService = recipeService;
        ToggleSaveCommand = new Command(ToggleSave);
    }

    // ✅ Called by Page when navigation data arrives
    public async Task SetRecipeAsync(Recipe recipe)
    {
        if (recipe == null)
            return;

        try
        {
            Recipe = recipe;
            IsSaved = _recipeService.IsRecipeSaved(recipe);
            await LoadDetailsAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("RecipeDetailsViewModel SetRecipeAsync crashed:");
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            throw; // rethrow so the Page catch can show it
        }
    }

    public async Task LoadDetailsAsync()
    {
        if (Recipe == null)
            return;

        if (_recipeService is ApiRecipeService api)
        {
            var fullRecipe = await api.GetRecipeDetailsAsync(Recipe.Id);

            // fullRecipe could be null depending on your implementation
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
}
