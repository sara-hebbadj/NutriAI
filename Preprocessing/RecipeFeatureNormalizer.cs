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

        private static float Normalize(float v, float min, float max) =>
            max == min ? 0 : (v - min) / (max - min);
    }

}
