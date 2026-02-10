using NutriAI.Models;

namespace NutriAI.Services.Recommendation;

public static class RecommendationExplainer
{
    public static List<string> Explain(
        Recipe recipe,
        UserRecipeInteraction interaction,
        MealContext context,
        UserPreferences preferences)
    {
        var reasons = new List<string>();
        var currentMeal = ContextHelper.ToMealTypeString(context);

        // =========================
        // 1️⃣ STRONGEST SIGNALS FIRST
        // =========================

        if (interaction.CookCount > 0)
        {
            reasons.Add("You cooked this recipe before");

            if (!string.IsNullOrWhiteSpace(interaction.LastUsedMealType))
            {
                reasons.Add(
                    $"You usually cook this for {interaction.LastUsedMealType}");
            }
        }
        else if (interaction.SaveCount > 0)
        {
            reasons.Add("You saved this recipe");
        }
        else if (interaction.ViewCount >= 2)
        {
            reasons.Add("You viewed similar recipes recently");
        }

        // =========================
        // 2️⃣ CONTEXT (TIME OF DAY)
        // =========================

        if (!string.IsNullOrWhiteSpace(recipe.MealType) &&
            recipe.MealType.Equals(currentMeal,
                StringComparison.OrdinalIgnoreCase))
        {
            reasons.Add($"Good match for {currentMeal}");
        }

        // =========================
        // 3️⃣ DIETARY PREFERENCES
        // =========================

        if (preferences.DietaryPreferences.Any())
        {
            if (!string.IsNullOrWhiteSpace(recipe.Diet) &&
                preferences.DietaryPreferences.Contains(
                    recipe.Diet.ToLowerInvariant()))
            {
                reasons.Add($"Matches your {recipe.Diet} preference");
            }
            else
            {
                reasons.Add("Fits your dietary preferences");
            }
        }

        // =========================
        // 4️⃣ HEALTH GOAL (SOFT)
        // =========================

        if (!string.IsNullOrWhiteSpace(preferences.Goal))
        {
            if (preferences.Goal == "lose weight" &&
                preferences.DailyCalorieTarget != null &&
                recipe.Calories <= preferences.DailyCalorieTarget)
            {
                reasons.Add("Supports your weight loss goal");
            }

            if (preferences.Goal == "gain muscle" &&
                recipe.ProteinGrams >= 25)
            {
                reasons.Add("High in protein for muscle gain");
            }
        }

        // =========================
        // FINAL OUTPUT
        // =========================

        if (reasons.Count == 0)
            reasons.Add("Recommended based on your activity");

        return reasons
            .Distinct()
            .Take(3)
            .ToList();
    }
}
