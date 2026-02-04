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
        var hour = DateTime.Now.Hour;

        if (hour >= 5 && hour < 11) return MealContext.Breakfast;
        if (hour >= 11 && hour < 16) return MealContext.Lunch;
        if (hour >= 16 && hour < 21) return MealContext.Dinner;

        return MealContext.Snack;
    }

    public static string ToMealTypeString(MealContext ctx)
        => ctx.ToString().ToLowerInvariant(); // "breakfast"
}

