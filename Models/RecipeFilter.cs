// Authorship note:
// This file was written by the author to hold recipe filtering options used in NutriAI.
// External help was minimal and limited to checking standard C# property syntax and nullable
// value usage for optional filter values.
// The filter fields themselves and their role in the app were chosen by the author.

namespace NutriAI.Models;

public class RecipeFilter
{
    public string MealType { get; set; } = string.Empty;

    public string Diet { get; set; } = string.Empty;

    public string Cuisine { get; set; } = string.Empty;

    // Nullable so that a calorie limit can be left unset.
    public int? MaxCalories { get; set; }
}