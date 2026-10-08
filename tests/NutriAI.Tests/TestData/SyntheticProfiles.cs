using NutriAI.Models;

namespace NutriAI.Tests.TestData;

public sealed record SyntheticProfile(
    string Name,
    UserPreferences Preferences,
    List<UserRecipeInteraction> Interactions);

// 30 synthetic users. The settings rotate through every option the app's profile pages offer,
// so each allergy, diet and goal appears several times. Interaction history comes from a
// seeded random generator, so every run produces exactly the same profiles.
public static class SyntheticProfiles
{
    // The exact strings the app stores (see AllergiesViewModel, DietaryPreferencesViewModel, HealthGoalsViewModel).
    public static readonly string[] AllergyOptions = { "nuts", "dairy", "eggs", "shellfish", "gluten" };
    public static readonly string[] DietOptions = { "", "vegetarian", "vegan", "halal", "gluten-free", "dairy-free", "keto" };
    public static readonly string[] GoalOptions = { "lose weight", "maintain", "gain muscle" };

    public static List<SyntheticProfile> Build(IReadOnlyList<LabelledRecipe> catalogue, int count = 30)
    {
        var profiles = new List<SyntheticProfile>();

        for (var i = 0; i < count; i++)
        {
            var prefs = new UserPreferences { Name = $"Test user {i + 1:00}" };

            // Allergies: one per profile in rotation, a second one for every third profile,
            // and none for every sixth profile (so allergy-free users are covered too).
            if (i % 6 != 5)
            {
                prefs.Allergies.Add(AllergyOptions[i % 5]);
                if (i % 3 == 0)
                    prefs.Allergies.Add(AllergyOptions[(i + 2) % 5]);
            }

            var diet = DietOptions[i % 7];
            if (diet != "")
                prefs.DietaryPreferences.Add(diet);

            prefs.Goal = GoalOptions[i % 3];
            prefs.DailyCalorieTarget = prefs.Goal switch
            {
                "lose weight" => 1800,
                "gain muscle" => 2600,
                _ => null,
            };

            profiles.Add(new SyntheticProfile(prefs.Name, prefs, MakeHistory(catalogue, seed: 1000 + i)));
        }

        return profiles;
    }

    // Six recipes the user has opened, some saved, some cooked, within the last 30 days.
    // History can include recipes that are unsafe for the user (for example a peanut dish cooked
    // before the allergy was entered). The ranking must still exclude those.
    private static List<UserRecipeInteraction> MakeHistory(IReadOnlyList<LabelledRecipe> catalogue, int seed)
    {
        var random = new Random(seed);
        var now = DateTime.UtcNow;
        var picked = catalogue.OrderBy(_ => random.Next()).Take(6);
        var history = new List<UserRecipeInteraction>();

        foreach (var item in picked)
        {
            var row = new UserRecipeInteraction
            {
                RecipeId = item.Recipe.Id,
                ViewCount = random.Next(1, 5),
                SaveCount = random.NextDouble() < 0.3 ? 1 : 0,
                CookCount = random.NextDouble() < 0.2 ? random.Next(1, 3) : 0,
                LastViewedAt = now.AddDays(-random.Next(0, 31)),
            };

            if (row.CookCount > 0)
            {
                row.LastCookedAt = now.AddDays(-random.Next(0, 31));
                row.LastUsedMealType = item.Recipe.MealType;
            }

            history.Add(row);
        }

        return history;
    }
}
