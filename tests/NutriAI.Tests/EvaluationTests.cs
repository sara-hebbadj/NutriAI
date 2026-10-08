using NutriAI.Tests.Helpers;
using Xunit.Abstractions;

namespace NutriAI.Tests;

// The 30-profile offline evaluation. It prints a Markdown report and saves it as
// evaluation-report.md next to the test binaries. Set NUTRIAI_EVAL_REPORT to a file path
// to save it somewhere else (that is how evals/evaluation-2026-10-08.md was produced).
public class EvaluationTests(ITestOutputHelper output)
{
    [Fact]
    public void Thirty_profile_evaluation_runs_and_every_unsafe_result_is_a_documented_finding()
    {
        var fullAudit = Evaluation.AuditRules(titleOnly: false);
        var titleAudit = Evaluation.AuditRules(titleOnly: true);
        var full = Evaluation.RunProfiles(titleOnly: false);
        var titleOnly = Evaluation.RunProfiles(titleOnly: true);

        var report = Evaluation.Report(fullAudit, titleAudit, full, titleOnly);
        output.WriteLine(report);
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "evaluation-report.md"), report);
        var extraPath = Environment.GetEnvironmentVariable("NUTRIAI_EVAL_REPORT");
        if (!string.IsNullOrWhiteSpace(extraPath))
            File.WriteAllText(extraPath, report);

        Assert.Equal(30, full.Profiles);
        Assert.Equal(120, full.Lists);

        // With full ingredient lists, every allergen or diet word the filter misses must be one
        // already listed in KnownFindings.MissedWithFullIngredients (finding F2).
        // A new miss fails this test; a fixed miss also fails it, so the ledger stays accurate.
        var actualMisses = fullAudit
            .SelectMany(r => r.Missed.Select(id => (r.Rule, id)))
            .OrderBy(x => x.Rule).ThenBy(x => x.id)
            .ToList();
        var expectedMisses = KnownFindings.MissedWithFullIngredients
            .Select(x => (x.Rule, x.RecipeId))
            .OrderBy(x => x.Rule).ThenBy(x => x.RecipeId)
            .ToList();
        Assert.Equal(expectedMisses, actualMisses);
    }
}
