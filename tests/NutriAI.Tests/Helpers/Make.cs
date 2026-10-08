using NutriAI.Models;

namespace NutriAI.Tests.Helpers;

// Small builders that keep the tests short and readable.
public static class Make
{
    public static Recipe Recipe(
        string title = "Plain Rice Bowl",
        string meal = "",
        int kcal = 400, int protein = 15, int carbs = 40, int fat = 10, int minutes = 20,
        params string[] ingredients) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Title = title,
        MealType = meal,
        Calories = kcal,
        ProteinGrams = protein,
        CarbsGrams = carbs,
        FatGrams = fat,
        CookingTimeMinutes = minutes,
        Ingredients = ingredients.ToList(),
    };

    // Normalised features. All 0 by default, which makes every soft weight neutral.
    public static RecipeFeatureVector Features(float calories = 0, float protein = 0, float cookingTime = 0) => new()
    {
        Calories = calories,
        Protein = protein,
        CookingTime = cookingTime,
    };

    public static UserRecipeInteraction NoHistory(Recipe r) => new() { RecipeId = r.Id };

    public static UserRecipeInteraction History(
        Recipe r, int views = 0, int saves = 0, int cooks = 0, double? viewedDaysAgo = null, double? cookedDaysAgo = null) => new()
    {
        RecipeId = r.Id,
        ViewCount = views,
        SaveCount = saves,
        CookCount = cooks,
        LastViewedAt = viewedDaysAgo is null ? null : DateTime.UtcNow.AddDays(-viewedDaysAgo.Value),
        LastCookedAt = cookedDaysAgo is null ? null : DateTime.UtcNow.AddDays(-cookedDaysAgo.Value),
    };

    public static UserPreferences Prefs(
        string[]? allergies = null, string[]? diets = null, string goal = "", int? calorieTarget = null) => new()
    {
        Allergies = (allergies ?? Array.Empty<string>()).ToList(),
        DietaryPreferences = (diets ?? Array.Empty<string>()).ToList(),
        Goal = goal,
        DailyCalorieTarget = calorieTarget,
    };
}
