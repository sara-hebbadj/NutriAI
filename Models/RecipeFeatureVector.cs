// Authorship note:
// This file was written by the author to support NutriAI's feature-based recipe representation.
// External help was limited to general C# class/property syntax and basic formatting.
// The decision to represent recipes using normalized nutrition/cooking features and encoded
// category fields was made by the author for the application's recommendation logic.

namespace NutriAI.Models;

public class RecipeFeatureVector
{
    // Links this feature vector back to the original recipe.
    public string RecipeId { get; set; } = string.Empty;

    // Normalized numeric features used for comparison/scoring.
    public float Calories { get; set; }
    public float Protein { get; set; }
    public float Carbs { get; set; }
    public float Fat { get; set; }
    public float CookingTime { get; set; }

    // Encoded category values used in a simplified numeric form.
    public int MealType { get; set; }
    public int Diet { get; set; }
    public int Cuisine { get; set; }
}