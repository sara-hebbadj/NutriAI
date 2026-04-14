// Authorship note:
// Standard C# date/time usage in this file was supported by Microsoft documentation.
// The meal-context ranges used by NutriAI (breakfast, lunch, dinner, snack)
// were chosen by the author for context-aware recommendation.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriAI.Services.Recommendation;

public static class ContextHelper
{
    public static MealContext GetCurrentMealContext()
    {
        // Use the current hour to estimate whether the user is likely
        // looking for breakfast, lunch, dinner, or a snack.
        var hour = DateTime.Now.Hour;

        if (hour >= 5 && hour < 11) return MealContext.Breakfast;
        if (hour >= 11 && hour < 16) return MealContext.Lunch;
        if (hour >= 16 && hour < 21) return MealContext.Dinner;

        return MealContext.Snack;
    }

    public static string ToMealTypeString(MealContext ctx)
        // Convert the enum value into the lowercase string format
        // used by the recipe data, for example "breakfast".
        => ctx.ToString().ToLowerInvariant(); // "breakfast"
}

