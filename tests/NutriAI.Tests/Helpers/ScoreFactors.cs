using NutriAI.Models;
using NutriAI.Services.Recommendation;

namespace NutriAI.Tests.Helpers;

// Splits RecipeScorer's score into its separate multipliers, so tests can ask
// "which factor mattered most?" and compare that with RecommendationExplainer's reasons.
//
// It re-writes the formula from Services/Recommendation/RecipeScorer.cs:
//   score = behaviour * recency * context * feature * keto * goal   (0 if a hard rule fails)
// ScoreFactorsTests checks that Total equals RecipeScorer.Score on thousands of inputs,
// so if someone changes the real formula, that test fails and this file must be updated.
public sealed record ScoreFactors(
    double Behaviour, double Recency, double Context, double Feature, double Keto, double Goal, bool Excluded)
{
    // Score of a recipe the user never touched: 0.5 baseline x 0.3 "unknown recency" weight.
    public const double ColdStart = 0.5 * 0.3;

    public double Total => Excluded ? 0.0 : Behaviour * Recency * Context * Feature * Keto * Goal;

    // How much the interaction history lifts the score compared with a never-seen recipe.
    public double BehaviourLift => Behaviour * Recency / ColdStart;

    // Goal-related multipliers together (normalised protein/calorie weight x goal weight).
    public double GoalLift => Feature * Goal;

    public static ScoreFactors Compute(
        Recipe recipe, RecipeFeatureVector f, UserRecipeInteraction i, MealContext context, UserPreferences prefs)
    {
        // Hard rules: the real scorer returns exactly 0.0 when a rule fails.
        var excluded = RecipeScorer.Score(recipe, f, i, context, prefs) == 0.0;

        var behaviour = 0.5 + 0.1 * i.ViewCount + 0.6 * i.SaveCount + 1.2 * i.CookCount;
        var recency = TimeDecay.Compute(i.LastCookedAt ?? i.LastViewedAt, halfLifeDays: 7);

        var meal = ContextHelper.ToMealTypeString(context);
        var contextMatch =
            recipe.MealType.Equals(meal, StringComparison.OrdinalIgnoreCase) ||
            (recipe.MealType.Equals("meal", StringComparison.OrdinalIgnoreCase) && (meal == "lunch" || meal == "dinner"));
        var contextWeight = !string.IsNullOrWhiteSpace(recipe.MealType) && contextMatch ? 1.25 : 1.0;

        var feature = 1.0;
        if (prefs.Goal == "gain muscle") feature *= 1.0 + f.Protein * 0.5;
        if (prefs.Goal == "lose weight") feature *= 1.2 - f.Calories * 0.5;
        if (f.CookingTime > 0.8) feature *= 0.9;

        var keto = !prefs.DietaryPreferences.Contains("keto") ? 1.0 : recipe.CarbsGrams <= 20 ? 1.3 : 0.4;

        var goal = prefs.Goal switch
        {
            "lose weight" when prefs.DailyCalorieTarget != null && recipe.Calories <= prefs.DailyCalorieTarget => 1.25,
            "lose weight" => 0.8,
            "gain muscle" when recipe.ProteinGrams >= 25 => 1.3,
            "gain muscle" => 0.9,
            _ => 1.0,
        };

        return new ScoreFactors(behaviour, recency, contextWeight, feature, keto, goal, excluded);
    }
}
