using NutriAI.Models;
using System.Collections.ObjectModel;

namespace NutriAI.Services;

public class MockRecipeService : IRecipeService
{
    private readonly ObservableCollection<Recipe> _allRecipes;
    private readonly ObservableCollection<Recipe> _savedRecipes;

    public static ObservableCollection<Recipe> All { get; internal set; }

    public MockRecipeService()
    {
        _savedRecipes = new ObservableCollection<Recipe>();

        _allRecipes = new ObservableCollection<Recipe>
        {
            new Recipe
            {
            ImageUrl = "https://cheffrecipes.com/wp-content/uploads/2024/07/Lemon-Herb-Grilled-Chicken-with-Couscous-1536x1536.jpg",
            Title = "Grilled Chicken with Quinoa",
            Calories = 420,
            CookingTimeMinutes = 25,
            ProteinGrams = 35,
            CarbsGrams = 45,
            FatGrams = 12,
            Ingredients =
            {
                "200g chicken breast",
                "1 cup cooked quinoa",
                "1 tbsp olive oil",
                "Salt",
                "Black pepper",
                "Paprika"
            },
            Instructions =
            {
                "Season the chicken breast with salt, pepper, and paprika.",
                "Heat olive oil in a grill pan over medium heat.",
                "Grill chicken for 6–7 minutes per side until fully cooked.",
                "Cook quinoa according to package instructions.",
                "Serve grilled chicken on top of quinoa."
            }
        },

        new Recipe
        {
            ImageUrl = "https://betterhomerecipes.com/wp-content/uploads/2025/09/Avocado_Egg_Toast_fzmctp.webp",
            Title = "Avocado Egg Toast",
            Calories = 320,
            CookingTimeMinutes = 10,
            ProteinGrams = 18,
            CarbsGrams = 30,
            FatGrams = 15,
            Ingredients =
            {
                "2 slices whole grain bread",
                "1 ripe avocado",
                "2 eggs",
                "Salt",
                "Chili flakes (optional)"
            },
            Instructions =
            {
                "Toast the bread slices.",
                "Mash avocado with salt.",
                "Cook eggs to preference (poached or fried).",
                "Spread avocado on toast.",
                "Top with eggs and chili flakes."
            }
        },

        new Recipe
        {
            ImageUrl = "https://tse3.mm.bing.net/th/id/OIP.VEWTe90ylhYXRrOfVi2FmgHaHa?rs=1&pid=ImgDetMain&o=7&rm=3",
            Title = "Salmon with Roasted Vegetables",
            Calories = 520,
            CookingTimeMinutes = 30,
            ProteinGrams = 40,
            CarbsGrams = 28,
            FatGrams = 22,
            Ingredients =
            {
                "150g salmon fillet",
                "Zucchini",
                "Bell peppers",
                "Carrots",
                "Olive oil",
                "Garlic",
                "Lemon"
            },
            Instructions =
            {
                "Preheat oven to 200°C.",
                "Chop vegetables and toss with olive oil and garlic.",
                "Roast vegetables for 20 minutes.",
                "Season salmon and bake for 10–12 minutes.",
                "Serve salmon with roasted vegetables and lemon."
            }
        }
    };

    }

    public ObservableCollection<Recipe> GetAllRecipes() => _allRecipes;

    public ObservableCollection<Recipe> GetSavedRecipes() => _savedRecipes;

    public void SaveRecipe(Recipe recipe)
    {
        if (!_savedRecipes.Contains(recipe))
            _savedRecipes.Add(recipe);
    }

    public void UnsaveRecipe(Recipe recipe)
    {
        if (_savedRecipes.Contains(recipe))
            _savedRecipes.Remove(recipe);
    }

    public bool IsRecipeSaved(Recipe recipe) =>
        _savedRecipes.Contains(recipe);
}
