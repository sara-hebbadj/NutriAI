using NutriAI.Models;

namespace NutriAI.ViewModels;

public class RecipeDetailsViewModel
{
    public Recipe Recipe { get; }

    public RecipeDetailsViewModel(Recipe recipe)
    {
        Recipe = recipe;
    }
}
