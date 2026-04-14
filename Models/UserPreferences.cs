// Authorship note:
// This file was written by the author to represent the user preference profile used by NutriAI.
// External help was limited to standard C# syntax support for properties, lists, and nullable values.
// The structure of the profile, including health goal, dietary preferences, allergies,
// calorie target, and cuisine preference, was designed by the author to support personalised
// recipe filtering and recommendation.

namespace NutriAI.Models;

public class UserPreferences
{
    public string Name { get; set; } = string.Empty;

    // These are optional because the user may not enter all profile data.
    public int? WeightKg { get; set; }
    public int? HeightCm { get; set; }

    public string Goal { get; set; } = string.Empty;  // lose weight, gain muscle

    // Stored as simple lists because NutriAI uses them directly in filtering logic.
    public List<string> DietaryPreferences { get; set; } = new(); // vegan, halal, etc.

    public List<string> Allergies { get; set; } = new();

    public int? DailyCalorieTarget { get; set; }

    public string PreferredCuisine { get; set; } = string.Empty;
}