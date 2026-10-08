using NutriAI.Models;
using NutriAI.Tests.TestData;

namespace NutriAI.Tests.Helpers;

// Decides, from the hand-written labels (not from the app's keyword lists),
// whether a recipe is unsafe for a user's allergies or hard dietary rules.
public static class SafetyOracle
{
    public static Contains ForbiddenFor(UserPreferences prefs)
    {
        var forbidden = Contains.None;

        foreach (var allergy in prefs.Allergies)
        {
            forbidden |= allergy switch
            {
                "nuts" => Contains.Nuts,
                "dairy" => Contains.Dairy,
                "eggs" => Contains.Egg,
                "shellfish" => Contains.Shellfish,
                "gluten" => Contains.Gluten,
                _ => throw new ArgumentException($"Unknown allergy option '{allergy}'"),
            };
        }

        foreach (var diet in prefs.DietaryPreferences)
        {
            forbidden |= diet switch
            {
                "vegetarian" => Contains.Meat | Contains.Pork | Contains.Fish | Contains.Shellfish,
                "vegan" => Contains.Meat | Contains.Pork | Contains.Fish | Contains.Shellfish | Contains.Dairy | Contains.Egg,
                "halal" => Contains.Pork, // alcohol and non-halal slaughter are not modelled (neither are they in the app)
                "gluten-free" => Contains.Gluten,
                "dairy-free" => Contains.Dairy,
                "keto" => Contains.None,  // keto is a soft weight in RecipeScorer, not an exclusion
                _ => throw new ArgumentException($"Unknown diet option '{diet}'"),
            };
        }

        return forbidden;
    }

    // The parts of the recipe that break the user's rules (None when the recipe is safe).
    public static Contains Violations(LabelledRecipe recipe, UserPreferences prefs) =>
        recipe.Truth & ForbiddenFor(prefs);

    public static bool IsSafe(LabelledRecipe recipe, UserPreferences prefs) =>
        Violations(recipe, prefs) == Contains.None;
}
