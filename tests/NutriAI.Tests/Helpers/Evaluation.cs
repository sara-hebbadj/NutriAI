using System.Text;
using NutriAI.Models;
using NutriAI.Services.Recommendation;
using NutriAI.Tests.TestData;

namespace NutriAI.Tests.Helpers;

// The offline evaluation: runs Sara's real filter and ranking code on synthetic data
// and compares the output with the hand-written labels. No network, no API key.
public static class Evaluation
{
    // ---------- 1. Filter audit: each hard rule on its own, against all 52 recipes ----------

    public sealed record RuleResult(
        string Rule, int Unsafe, List<string> Missed, int Safe, List<string> FalselyExcluded);

    // The 10 hard rules a user can switch on in the app (5 allergies + 5 dietary rules; keto is soft).
    public static IEnumerable<(string Rule, UserPreferences Prefs)> HardRules()
    {
        foreach (var allergy in SyntheticProfiles.AllergyOptions)
            yield return ($"allergy: {allergy}", Make.Prefs(allergies: new[] { allergy }));

        foreach (var diet in new[] { "vegetarian", "vegan", "halal", "gluten-free", "dairy-free" })
            yield return ($"diet: {diet}", Make.Prefs(diets: new[] { diet }));
    }

    public static bool IsExcluded(Recipe recipe, UserPreferences prefs) =>
        RecipeScorer.Score(recipe, new RecipeFeatureVector(), new UserRecipeInteraction { RecipeId = recipe.Id },
            MealContext.Lunch, prefs) == 0.0;

    public static List<RuleResult> AuditRules(bool titleOnly)
    {
        var recipes = SyntheticRecipes.FreshCopy(titleOnly);
        var results = new List<RuleResult>();

        foreach (var (rule, prefs) in HardRules())
        {
            var unsafeRecipes = recipes.Where(x => !SafetyOracle.IsSafe(x, prefs)).ToList();
            var safeRecipes = recipes.Where(x => SafetyOracle.IsSafe(x, prefs)).ToList();

            results.Add(new RuleResult(
                rule,
                unsafeRecipes.Count,
                unsafeRecipes.Where(x => !IsExcluded(x.Recipe, prefs)).Select(x => x.Recipe.Id).ToList(),
                safeRecipes.Count,
                safeRecipes.Where(x => IsExcluded(x.Recipe, prefs)).Select(x => x.Recipe.Id).ToList()));
        }

        return results;
    }

    // ---------- 2. Thirty synthetic users x four meal times ----------

    public sealed record ProfileRunResult(
        string Condition,
        int Profiles,
        int Lists,
        int ListsAllSafe,
        int ListsTop5Safe,
        int ItemsShown,
        int UnsafeItemsShown,
        int Top5Items,
        int UnsafeTop5Items,
        int SafeCandidates,
        int SafeExcluded,
        int ListsWithFewerThan5,
        double MeanDistinctCuisinesTop5,
        int DistinctRecipesInAnyTop5,
        int CatalogueSize,
        Dictionary<string, (double Kcal, double Protein)> Top5NutritionByGoal,
        List<string> UnsafeShown);

    public static ProfileRunResult RunProfiles(bool titleOnly)
    {
        var catalogue = SyntheticRecipes.FreshCopy(titleOnly);
        var profiles = SyntheticProfiles.Build(catalogue);
        var byId = catalogue.ToDictionary(x => x.Recipe.Id);

        int lists = 0, listsAllSafe = 0, listsTop5Safe = 0, shown = 0, unsafeShown = 0;
        int top5Items = 0, unsafeTop5 = 0, safeCandidates = 0, safeExcluded = 0, short5 = 0;
        var cuisineCounts = new List<int>();
        var inAnyTop5 = new HashSet<string>();
        var unsafeExamples = new SortedSet<string>();
        var top5ByGoal = new Dictionary<string, List<Recipe>>();

        foreach (var profile in profiles)
        {
            // Over-exclusion does not depend on the meal time, so count it once per user.
            foreach (var item in catalogue.Where(x => SafetyOracle.IsSafe(x, profile.Preferences)))
            {
                safeCandidates++;
                if (IsExcluded(item.Recipe, profile.Preferences)) safeExcluded++;
            }

            foreach (var context in Enum.GetValues<MealContext>())
            {
                // Fresh recipe objects each time: Rank writes RecommendationReasons onto them.
                var candidates = SyntheticRecipes.FreshCopy(titleOnly).Select(x => x.Recipe).ToList();
                var ranked = RecommendationService.Rank(candidates, profile.Preferences, profile.Interactions, context);

                lists++;
                var top5 = ranked.Take(5).ToList();
                if (top5.Count < 5) short5++;

                var unsafeInList = ranked.Where(r => !SafetyOracle.IsSafe(byId[r.Id], profile.Preferences)).ToList();
                var unsafeInTop5 = top5.Where(r => !SafetyOracle.IsSafe(byId[r.Id], profile.Preferences)).ToList();

                shown += ranked.Count;
                unsafeShown += unsafeInList.Count;
                top5Items += top5.Count;
                unsafeTop5 += unsafeInTop5.Count;
                if (unsafeInList.Count == 0) listsAllSafe++;
                if (unsafeInTop5.Count == 0) listsTop5Safe++;

                foreach (var r in unsafeInList)
                    unsafeExamples.Add($"{r.Id} ({SafetyOracle.Violations(byId[r.Id], profile.Preferences)})");

                if (top5.Count > 0) cuisineCounts.Add(top5.Select(r => r.Cuisine).Distinct().Count());
                if (!top5ByGoal.ContainsKey(profile.Preferences.Goal)) top5ByGoal[profile.Preferences.Goal] = new List<Recipe>();
                top5ByGoal[profile.Preferences.Goal].AddRange(top5);
                foreach (var r in top5) inAnyTop5.Add(r.Id);
            }
        }

        return new ProfileRunResult(
            titleOnly ? "title only (as the home feed maps Spoonacular results)" : "full ingredient lists",
            profiles.Count, lists, listsAllSafe, listsTop5Safe, shown, unsafeShown, top5Items, unsafeTop5,
            safeCandidates, safeExcluded, short5,
            cuisineCounts.Count == 0 ? double.NaN : cuisineCounts.Average(), inAnyTop5.Count, catalogue.Count,
            top5ByGoal.ToDictionary(g => g.Key, g => g.Value.Count == 0
                ? (double.NaN, double.NaN)
                : (g.Value.Average(r => (double)r.Calories), g.Value.Average(r => (double)r.ProteinGrams))),
            unsafeExamples.ToList());
    }

    // ---------- 3. Markdown report ----------

    public static string Report(
        List<RuleResult> fullAudit, List<RuleResult> titleAudit, ProfileRunResult full, ProfileRunResult titleOnly)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# NutriAI offline evaluation report");
        sb.AppendLine();
        sb.AppendLine($"Generated (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm}. Data: 52 synthetic labelled recipes, 30 synthetic users (seeded), 4 meal times.");
        sb.AppendLine("Code under test: Services/Recommendation/RecipeScorer.cs, RecommendationService.cs and RecommendationExplainer.cs.");
        sb.AppendLine("Produced by: dotnet test tests/NutriAI.Tests (EvaluationTests). Deterministic: same numbers on every run.");
        sb.AppendLine();
        sb.AppendLine("## 1. Hard-rule filter audit (each rule alone, 52 recipes)");
        sb.AppendLine();
        AppendAudit(sb, "Full ingredient lists", fullAudit);
        AppendAudit(sb, "Title only (home feed data)", titleAudit);
        sb.AppendLine("## 2. Thirty users x four meal times = 120 recommendation lists");
        sb.AppendLine();
        sb.AppendLine("| Measure | Full ingredient lists | Title only (home feed) |");
        sb.AppendLine("|---|---|---|");
        Row(sb, "Lists where every recommended recipe is safe", Pct(full.ListsAllSafe, full.Lists), Pct(titleOnly.ListsAllSafe, titleOnly.Lists));
        Row(sb, "Lists whose top 5 are all safe", Pct(full.ListsTop5Safe, full.Lists), Pct(titleOnly.ListsTop5Safe, titleOnly.Lists));
        Row(sb, "Unsafe recipes among all recommended", Pct(full.UnsafeItemsShown, full.ItemsShown), Pct(titleOnly.UnsafeItemsShown, titleOnly.ItemsShown));
        Row(sb, "Unsafe recipes among top-5 slots", Pct(full.UnsafeTop5Items, full.Top5Items), Pct(titleOnly.UnsafeTop5Items, titleOnly.Top5Items));
        Row(sb, "Safe recipes wrongly excluded (per user)", Pct(full.SafeExcluded, full.SafeCandidates), Pct(titleOnly.SafeExcluded, titleOnly.SafeCandidates));
        Row(sb, "Top-5 diversity: mean distinct cuisines (of 5 cuisine groups; non-empty lists)", $"{full.MeanDistinctCuisinesTop5:0.00}", $"{titleOnly.MeanDistinctCuisinesTop5:0.00}");
        Row(sb, "Top-5 coverage: recipes appearing in any top 5", $"{full.DistinctRecipesInAnyTop5} / {full.CatalogueSize}", $"{titleOnly.DistinctRecipesInAnyTop5} / {titleOnly.CatalogueSize}");
        Row(sb, "Lists with fewer than 5 recipes", $"{full.ListsWithFewerThan5} / {full.Lists}", $"{titleOnly.ListsWithFewerThan5} / {titleOnly.Lists}");
        foreach (var goal in new[] { "lose weight", "maintain", "gain muscle" })
            Row(sb, $"Goal effect: mean kcal / protein (g) of top-5 recipes, \"{goal}\" users",
                Nutrition(full.Top5NutritionByGoal[goal]), Nutrition(titleOnly.Top5NutritionByGoal[goal]));
        sb.AppendLine();
        sb.AppendLine($"Catalogue average: {SyntheticRecipes.All.Average(x => x.Recipe.Calories):0} kcal / {SyntheticRecipes.All.Average(x => x.Recipe.ProteinGrams):0.0} g protein.");
        sb.AppendLine();
        sb.AppendLine($"Unsafe recipes shown, full ingredient lists: {string.Join(", ", full.UnsafeShown)}");
        sb.AppendLine();
        sb.AppendLine($"Unsafe recipes shown, title only: {string.Join(", ", titleOnly.UnsafeShown)}");
        return sb.ToString();
    }

    private static void AppendAudit(StringBuilder sb, string title, List<RuleResult> audit)
    {
        sb.AppendLine($"### {title}");
        sb.AppendLine();
        sb.AppendLine("| Rule | Unsafe recipes | Missed (unsafe but kept) | Safe recipes | Wrongly excluded |");
        sb.AppendLine("|---|---|---|---|---|");
        foreach (var r in audit)
            sb.AppendLine($"| {r.Rule} | {r.Unsafe} | {r.Missed.Count} {Ids(r.Missed)} | {r.Safe} | {r.FalselyExcluded.Count} {Ids(r.FalselyExcluded)} |");
        var missed = audit.Sum(r => r.Missed.Count);
        var unsafeTotal = audit.Sum(r => r.Unsafe);
        var wrong = audit.Sum(r => r.FalselyExcluded.Count);
        var safeTotal = audit.Sum(r => r.Safe);
        sb.AppendLine($"| **All rules** | {unsafeTotal} | **{missed}** ({Pct(missed, unsafeTotal)}) | {safeTotal} | **{wrong}** ({Pct(wrong, safeTotal)}) |");
        sb.AppendLine();
    }

    private static string Nutrition((double Kcal, double Protein) n) =>
        double.IsNaN(n.Kcal) ? "n/a (no recipes shown)" : $"{n.Kcal:0} kcal / {n.Protein:0.0} g";

    private static string Ids(List<string> ids) => ids.Count == 0 ? "" : $"({string.Join(", ", ids)})";
    private static void Row(StringBuilder sb, string name, string a, string b) => sb.AppendLine($"| {name} | {a} | {b} |");
    public static string Pct(int part, int whole) =>
        whole == 0 ? "n/a" : $"{part}/{whole} = {100.0 * part / whole:0.0}%";
}
