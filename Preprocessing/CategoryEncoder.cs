// Authorship note:
// Microsoft documentation was used for standard C# switch expressions in this file.
// Copilot was used to help draft and refine the category-encoding structure.
// The NutriAI-specific category groupings and the numeric mappings for meal type,
// diet, and cuisine were chosen by the author for use in recipe feature preprocessing.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriAI.Preprocessing;

public static class CategoryEncoder
{
    // Convert meal-type labels into simple numeric values
    // so they can be stored in the feature vector.
    public static int EncodeMealType(string mealType)
    {
        return mealType switch
        {
            "breakfast" => 0,
            "lunch" => 1,
            "dinner" => 2,
            "snack" => 3,
            _ => -1 // unknown / other
        };
    }

    // Convert diet labels into numeric values used by the preprocessing step.
    public static int EncodeDiet(string diet)
    {
        return diet switch
        {
            "balanced" => 0,
            "vegetarian" => 1,
            "vegan" => 2,
            "keto" => 3,
            "pescatarian" => 4,
            "gluten free" => 5,
            "dairy free" => 6,
            _ => -1
        };
    }

    // Convert cuisine labels into a smaller set of numeric categories.
    public static int EncodeCuisine(string cuisine)
    {
        return cuisine switch
        {
            "asian" => 0,
            "italian" => 1,
            "middle eastern" => 2,
            "american" => 3,
            "other" => 4,
            _ => -1
        };
    }
}