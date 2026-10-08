using NutriAI.Models;
using NutriAI.Services.Recommendation;
using NutriAI.Tests.Helpers;
using NutriAI.Tests.TestData;

namespace NutriAI.Tests;

// Property-style checks: rules that must hold for every input, tried on many generated inputs.
public class GoalAndSafetyPropertyTests
{
    private static readonly Random Seeded = new(2026);

    // ---------- Safety: an excluded recipe can never come back ----------

    [Fact]
    public void A_recipe_the_filter_excludes_never_appears_however_much_the_user_liked_it()
    {
        // Every synthetic user, every meal time, and a history that cooked EVERY recipe today.
        // History must not be able to override an allergy or a dietary rule.
        var catalogue = SyntheticRecipes.FreshCopy();
        foreach (var profile in SyntheticProfiles.Build(catalogue))
        {
            var lovedEverything = catalogue
                .Select(x => Make.History(x.Recipe, views: 5, saves: 2, cooks: 3, viewedDaysAgo: 0, cookedDaysAgo: 0))
                .ToList();

            foreach (var context in Enum.GetValues<MealContext>())
            {
                var recipes = SyntheticRecipes.FreshCopy().Select(x => x.Recipe).ToList();
                var ranked = RecommendationService.Rank(recipes, profile.Preferences, lovedEverything, context);

                Assert.All(ranked, r => Assert.False(Evaluation.IsExcluded(r, profile.Preferences)));
            }
        }
    }

    [Fact]
    public void Every_allergy_option_in_the_app_is_understood_by_the_scorer()
    {
        // The Allergies page stores exactly these strings. Each must trigger its keyword group.
        var cases = new Dictionary<string, string>
        {
            ["nuts"] = "50 g walnuts", ["dairy"] = "1 cup milk", ["eggs"] = "2 eggs",
            ["shellfish"] = "200 g shrimp", ["gluten"] = "1 cup flour",
        };
        Assert.Equal(SyntheticProfiles.AllergyOptions.OrderBy(x => x), cases.Keys.OrderBy(x => x));

        foreach (var (allergy, ingredient) in cases)
        {
            var r = Make.Recipe("Test Dish", ingredients: ingredient);
            Assert.True(Evaluation.IsExcluded(r, Make.Prefs(allergies: new[] { allergy })), allergy);
        }
    }

    // ---------- Goals: soft preferences that must push in the right direction ----------
    // Each test compares two recipes that are identical except for the goal-relevant value.

    [Fact]
    public void Lose_weight_within_the_calorie_target_never_scores_below_an_identical_recipe_above_it()
    {
        for (var n = 0; n < 1000; n++)
        {
            var target = Seeded.Next(300, 2500);
            var within = Make.Recipe(kcal: Seeded.Next(100, target + 1));
            var above = Make.Recipe(kcal: Seeded.Next(target + 1, target + 1500));
            var prefs = Make.Prefs(goal: "lose weight", calorieTarget: target);

            Assert.True(ScoreSameContext(within, prefs, calorieFeature: 0.3f) >= ScoreSameContext(above, prefs, calorieFeature: 0.3f));
            // Using the real normalised features too: higher calories never help.
            Assert.True(ScoreSameContext(within, prefs, calorieFeature: 0.2f) >= ScoreSameContext(above, prefs, calorieFeature: 0.9f));
        }
    }

    [Fact]
    public void Gain_muscle_never_prefers_the_lower_protein_recipe()
    {
        for (var n = 0; n < 1000; n++)
        {
            var high = Make.Recipe(protein: Seeded.Next(25, 80));
            var low = Make.Recipe(protein: Seeded.Next(0, 25));
            var prefs = Make.Prefs(goal: "gain muscle");

            Assert.True(ScoreSameContext(high, prefs, proteinFeature: 0.8f) >= ScoreSameContext(low, prefs, proteinFeature: 0.2f));
        }
    }

    [Fact]
    public void Keto_never_prefers_the_higher_carb_recipe()
    {
        for (var n = 0; n < 1000; n++)
        {
            var lowCarb = Make.Recipe(carbs: Seeded.Next(0, 21));
            var highCarb = Make.Recipe(carbs: Seeded.Next(21, 120));
            var prefs = Make.Prefs(diets: new[] { "keto" });

            Assert.True(ScoreSameContext(lowCarb, prefs) > ScoreSameContext(highCarb, prefs));
        }
    }

    private static double ScoreSameContext(Recipe r, UserPreferences prefs, float calorieFeature = 0, float proteinFeature = 0) =>
        RecipeScorer.Score(r, Make.Features(calorieFeature, proteinFeature), Make.NoHistory(r), MealContext.Lunch, prefs);
}
