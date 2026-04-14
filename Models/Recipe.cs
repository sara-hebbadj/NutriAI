// Authorship note:
// This model was written by the author as part of the NutriAI application data structure.
// External help was limited to general C# syntax reference, including auto-implemented properties,
// default value initialisation, and use of Guid.NewGuid() for generating a local recipe identifier.
// Microsoft Learn and tutorial-style resources were used only to confirm syntax.
// The choice of fields included here (nutrition values, ingredients, meal type, diet, cuisine,
// saved state, and recommendation reasons) was made by the author based on NutriAI's app and
// recommendation requirements.

namespace NutriAI.Models;

public class Recipe
{
    // A local default ID is created when a recipe object is initialised.
    // This helps the app keep track of recipe items consistently.
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Title { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public int Calories { get; set; }

    public int CookingTimeMinutes { get; set; }

    public int ProteinGrams { get; set; }

    public int CarbsGrams { get; set; }

    public int FatGrams { get; set; }

    // Ingredient names are stored as simple strings for display and filtering.
    public List<string> Ingredients { get; set; } = new();

    // Instructions are optional because some API results may not always include them.
    public string? Instructions { get; set; }

    // These category fields are used by filtering and recommendation logic.
    public string MealType { get; set; } = string.Empty;   // breakfast, lunch, dinner
    public string Diet { get; set; } = string.Empty;       // vegan, keto, etc.
    public string Cuisine { get; set; } = string.Empty;    // Italian, Asian, etc.

    public bool IsSaved { get; set; }

    // Stores short explanations shown to the user for why a recipe was recommended.
    public List<string> RecommendationReasons { get; set; } = new();
}