using System.Collections.ObjectModel;
using NutriAI.Models;

namespace NutriAI.ViewModels;

public class HomeViewModel
{
    public ObservableCollection<Recipe> Recipes { get; }

    public HomeViewModel()
    {
        Recipes = new ObservableCollection<Recipe>
        {
            new Recipe
            {
                Title = "Grilled Chicken with Quinoa",
                Calories = 420,
                CookingTimeMinutes = 25,
                ProteinGrams = 35,
                CarbsGrams = 45,
                FatGrams = 12
            },
            new Recipe
            {
                Title = "Avocado Egg Toast",
                Calories = 320,
                CookingTimeMinutes = 10,
                ProteinGrams = 14,
                CarbsGrams = 30,
                FatGrams = 18
            }
        };
    }
}
