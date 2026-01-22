namespace NutriAI.Models;

public class Recipe
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Title { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public int Calories { get; set; }

    public int CookingTimeMinutes { get; set; }

    public int ProteinGrams { get; set; }

    public int CarbsGrams { get; set; }

    public int FatGrams { get; set; }

    public List<string> Ingredients { get; set; } = new();

    public List<string> Instructions { get; set; } = new();

    public string MealType { get; set; } = string.Empty;   // breakfast, lunch, dinner
    public string Diet { get; set; } = string.Empty;       // vegan, keto, etc.
    public string Cuisine { get; set; } = string.Empty;    // Italian, Asian, etc.
    public bool IsSaved { get; set; }

}

