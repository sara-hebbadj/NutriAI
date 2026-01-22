using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NutriAI.Preprocessing;

public static class CategoryEncoder
{
    // ========================
    // MEAL TYPE
    // ========================
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

    // ========================
    // DIET
    // ========================
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

    // ========================
    // CUISINE
    // ========================
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

