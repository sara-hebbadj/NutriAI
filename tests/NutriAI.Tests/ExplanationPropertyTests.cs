using NutriAI.Models;
using NutriAI.Preprocessing;
using NutriAI.Services.Recommendation;
using NutriAI.Tests.Helpers;
using NutriAI.Tests.TestData;
using Xunit.Abstractions;

namespace NutriAI.Tests;

// Property-style checks for Services/Recommendation/RecommendationExplainer.cs:
// "the reasons shown to the user must match the factors that actually produced the score".
// 5,000 random (recipe, user, history, meal time) cases from a fixed seed.
public class ExplanationPropertyTests(ITestOutputHelper output)
{
    private const int Cases = 5000;

    private sealed record Case(Recipe Recipe, UserRecipeInteraction History, MealContext Context,
        UserPreferences Prefs, ScoreFactors Factors, List<string> Reasons);

    private static IEnumerable<Case> RandomCases()
    {
        var random = new Random(42);
        var recipes = SyntheticRecipes.FreshCopy().Select(x => x.Recipe).ToList();
        var stats = RankingTests.StatsFor(recipes);
        var goals = new[] { "", "lose weight", "maintain", "gain muscle" };

        var produced = 0;
        while (produced < Cases)
        {
            var recipe = recipes[random.Next(recipes.Count)];
            var prefs = Make.Prefs(
                diets: random.NextDouble() < 0.4 ? new[] { "keto" } : random.NextDouble() < 0.5 ? new[] { "halal" } : null,
                goal: goals[random.Next(goals.Length)],
                calorieTarget: random.NextDouble() < 0.5 ? 1800 : null);
            var history = RandomHistory(random, recipe);
            var context = (MealContext)random.Next(4);
            var features = RecipeFeatureNormalizer.FromRecipe(recipe, stats);
            var factors = ScoreFactors.Compute(recipe, features, history, context, prefs);

            if (factors.Excluded)
                continue; // excluded recipes are never shown, so they need no explanation

            var reasons = RecommendationExplainer.Explain(recipe, history, context, prefs);
            produced++;
            yield return new Case(recipe, history, context, prefs, factors, reasons);
        }
    }

    private static UserRecipeInteraction RandomHistory(Random random, Recipe recipe)
    {
        var h = new UserRecipeInteraction
        {
            RecipeId = recipe.Id,
            ViewCount = random.Next(0, 5),
            SaveCount = random.NextDouble() < 0.3 ? 1 : 0,
            CookCount = random.NextDouble() < 0.3 ? random.Next(1, 3) : 0,
        };
        if (h.ViewCount + h.SaveCount + h.CookCount > 0)
            h.LastViewedAt = DateTime.UtcNow.AddDays(-random.Next(0, 41));
        if (h.CookCount > 0)
        {
            h.LastCookedAt = DateTime.UtcNow.AddDays(-random.Next(0, 41));
            h.LastUsedMealType = ((MealContext)random.Next(4)).ToString().ToLowerInvariant();
        }
        return h;
    }

    // ---------- Claims that must always be true (strict: zero violations allowed) ----------

    [Fact]
    public void Every_claimed_reason_has_a_matching_factor()
    {
        foreach (var c in RandomCases())
        {
            foreach (var reason in c.Reasons)
            {
                if (reason == "You cooked this recipe before") Assert.True(c.History.CookCount > 0);
                if (reason == "You saved this recipe") Assert.True(c.History.SaveCount > 0);
                if (reason == "You viewed similar recipes recently") Assert.True(c.History.ViewCount >= 2);
                if (reason.StartsWith("Good match for")) Assert.Equal(1.25, c.Factors.Context);
                if (reason == "Supports your weight loss goal") Assert.Equal(1.25, c.Factors.Goal);
                if (reason == "High in protein for muscle gain") Assert.Equal(1.3, c.Factors.Goal);
            }
        }
    }

    [Fact]
    public void Explanations_are_short_and_never_empty()
    {
        Assert.All(RandomCases(), c => Assert.InRange(c.Reasons.Count, 1, 3));
    }

    // ---------- "Explanations match the top factors" (known finding F4) ----------

    [Fact]
    public void Explanation_mismatches_are_only_the_documented_kinds()
    {
        var counts = new Dictionary<string, int>();
        var all = RandomCases().ToList();

        foreach (var c in all)
            foreach (var kind in Mismatches(c))
                counts[kind] = counts.GetValueOrDefault(kind) + 1;

        output.WriteLine($"Explanation check on {all.Count} random shown recipes (seed 42):");
        foreach (var (kind, n) in counts.OrderByDescending(x => x.Value))
            output.WriteLine($"  {n,5} / {all.Count} = {100.0 * n / all.Count:0.0}%  {kind}");

        // Same set in both directions: no new kind of mismatch, and no listed kind that no longer happens.
        Assert.Equal(KnownFindings.ExplanationMismatchKinds.OrderBy(k => k), counts.Keys.OrderBy(k => k));
    }

    private static IEnumerable<string> Mismatches(Case c)
    {
        var f = c.Factors;
        var reasons = c.Reasons;
        bool Has(string text) => reasons.Any(r => r.StartsWith(text));

        var historyReason = Has("You cooked") || Has("You saved") || Has("You viewed");
        var dietReason = Has("Fits your dietary") || Has("Matches your");
        var goalReason = Has("Supports your weight loss") || Has("High in protein");

        if (dietReason && f.Keto < 1.0)
            yield return "diet reason shown although keto rule lowered the score";

        if (Has("You usually cook this for"))
            yield return "reason not used by the scorer (\"You usually cook this for ...\")";

        if (Has("You viewed similar recipes recently") && f.Recency < 0.5)
            yield return "\"recently\" claimed for a view older than 7 days";

        if (historyReason && f.BehaviourLift < 1.0)
            yield return "history reason shown although decayed history lowers the score";

        // The strongest factor that raised the score (by more than 5%) should be named.
        var lifts = new (string Name, double Lift, bool Mentioned)[]
        {
            ("history", f.BehaviourLift, historyReason),
            ("meal time", f.Context, Has("Good match for")),
            ("goal", f.GoalLift, goalReason),
            ("keto", f.Keto, dietReason),
        };
        var top = lifts.OrderByDescending(x => x.Lift).First();
        if (top.Lift > 1.05 && !top.Mentioned)
            yield return "strongest factor not mentioned";
    }
}
