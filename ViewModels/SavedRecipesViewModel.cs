using System.Collections.ObjectModel;
using NutriAI.Models;

namespace NutriAI.ViewModels;

public class SavedRecipesViewModel
{
    public ObservableCollection<Recipe> SavedRecipes { get; }

    public SavedRecipesViewModel()
    {
        SavedRecipes = new ObservableCollection<Recipe>
        {
            new Recipe
            {
                Title = "Protein Smoothie Bowl",
                Calories = 350,
                CookingTimeMinutes = 5,
                ProteinGrams = 25,
                CarbsGrams = 40,
                FatGrams = 8
            }
        };
    }
}
