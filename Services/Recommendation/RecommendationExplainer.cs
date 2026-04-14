// Authorship note:
// Microsoft documentation was used in this file for List<T>, LINQ methods,
// string checks, and string comparison.
// Copilot was used to help draft and refine the explanation building logic.
// The actual recommendation reasons, their priority order, and the decision to return
// short human readable explanations for NutriAI were defined by the author.

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

        // Start with the strongest behavioural signals first,
        // because these are the clearest indicators of user preference.
        if (interaction.CookCount > 0)
        {
            reasons.Add("You cooked this recipe before");

            // If the app knows the meal type previously used,
            // include that as extra behavioural context.
            if (!string.IsNullOrWhiteSpace(interaction.LastUsedMealType))
            {
                reasons.Add(
                    $"You usually cook this for {interaction.LastUsedMealType}");
            }
        }
        else if (interaction.SaveCount > 0)
        {
            // A save is treated as a strong sign of interest,
            // but weaker than actually cooking the recipe.
            reasons.Add("You saved this recipe");
        }
        else if (interaction.ViewCount >= 2)
        {
            // Repeated viewing suggests interest even if the recipe
            // has not yet been saved or cooked.
            reasons.Add("You viewed similar recipes recently");
        }

        // Add a context explanation if the recipe matches the current meal time.
        if (!string.IsNullOrWhiteSpace(recipe.MealType))
        {
            var isDirectMatch =
                recipe.MealType.Equals(currentMeal,
                    StringComparison.OrdinalIgnoreCase);

            var isFlexibleMealMatch =
                recipe.MealType.Equals("meal", StringComparison.OrdinalIgnoreCase) &&
                (currentMeal.Equals("lunch", StringComparison.OrdinalIgnoreCase) ||
                 currentMeal.Equals("dinner", StringComparison.OrdinalIgnoreCase));

            if (isDirectMatch || isFlexibleMealMatch)
            {
                reasons.Add($"Good match for {currentMeal}");
            }
        }

        // Add a simple dietary explanation if dietary preferences are active.
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

        // Add a health-goal explanation when the recipe supports
        // the user's current goal in a simple and readable way.
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

        // If no specific reason was triggered, return a generic fallback explanation.
        if (reasons.Count == 0)
            reasons.Add("Recommended based on your activity");

        // Remove duplicates and keep the explanation short enough for the UI.
        return reasons
            .Distinct()
            .Take(3)
            .ToList();
    }
}