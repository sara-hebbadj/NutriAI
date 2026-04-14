// Authorship note:
// Microsoft documentation was used in this file for standard C# methods,
// numeric operations, and class/object initialization syntax.
// Copilot was used to help draft and refine the feature-normalization structure.
// The NutriAI-specific preprocessing design - converting recipe attributes into a feature vector,
// using min-max normalization for nutrition values, and encoding meal type, diet, and cuisine
// into numeric form - was chosen and adapted by the author.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NutriAI.Models;
using NutriAI.Preprocessing;

namespace NutriAI.Preprocessing
{
    public static class RecipeFeatureNormalizer
    {
        public static RecipeFeatureVector FromRecipe(
            Recipe r,
            NutritionStats stats)
        {
            // Build a structured feature vector from the raw recipe data.
            // Numeric nutrition values are normalized,
            // while category fields are encoded into integer values.
            return new RecipeFeatureVector
            {
                RecipeId = r.Id,

                Calories = Normalize(r.Calories, stats.MinCalories, stats.MaxCalories),
                Protein = Normalize(r.ProteinGrams, stats.MinProtein, stats.MaxProtein),
                Carbs = Normalize(r.CarbsGrams, stats.MinCarbs, stats.MaxCarbs),
                Fat = Normalize(r.FatGrams, stats.MinFat, stats.MaxFat),
                CookingTime = Normalize(r.CookingTimeMinutes,
                                        stats.MinTime, stats.MaxTime),

                MealType = CategoryEncoder.EncodeMealType(r.MealType),
                Diet = CategoryEncoder.EncodeDiet(r.Diet),
                Cuisine = CategoryEncoder.EncodeCuisine(r.Cuisine)

            };
        }

        // Min-max normalization:
        // scales a value into the 0 to 1 range using the current dataset's min and max.
        // If min and max are the same, return 0 to avoid division by zero.
        private static float Normalize(float v, float min, float max) =>
            max == min ? 0 : (v - min) / (max - min);
    }

}