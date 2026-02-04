using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NutriAI.Models;

namespace NutriAI.Services.Recommendation;

public static class RecipeScorer
{
    public static double Score(
        Recipe recipe,
        UserRecipeInteraction interaction,
        MealContext context)
    {
        // 1) Strength of implicit feedback
        var baseScore =
              0.1 * interaction.ViewCount
            + 0.6 * interaction.SaveCount
            + 1.2 * interaction.CookCount;

        // 2) Time decay (prefer recent cooking; fall back to recent viewing)
        var lastEvent = interaction.LastCookedAt ?? interaction.LastViewedAt;
        var recencyWeight = TimeDecay.Compute(lastEvent, halfLifeDays: 7);

        // 3) Context boost: if recipe meal type matches current time-of-day
        var contextBoost = 1.0;
        var currentMealType = ContextHelper.ToMealTypeString(context);

        if (!string.IsNullOrWhiteSpace(recipe.MealType) &&
            recipe.MealType.Equals(currentMealType, StringComparison.OrdinalIgnoreCase))
        {
            contextBoost *= 1.25; // boost breakfast recipes in morning, etc.
        }

        // 4) Personal context boost: if user last cooked this recipe as this meal type
        if (!string.IsNullOrWhiteSpace(interaction.LastUsedMealType) &&
            interaction.LastUsedMealType.Equals(currentMealType, StringComparison.OrdinalIgnoreCase))
        {
            contextBoost *= 1.15;
        }

        return baseScore * recencyWeight * contextBoost;
    }
}

