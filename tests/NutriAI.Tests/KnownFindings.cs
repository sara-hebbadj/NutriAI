namespace NutriAI.Tests;

// Ledger of problems the tests found in the dissertation code (October 2026).
// Nothing here is hidden: each finding is explained in docs/architecture.md ("Known issues"), has its own test in
// KnownFindingsTests.cs, and a proposed fix in proposed-fixes/ (not applied to the app code).
//
// Tests compare the real behaviour with this ledger in BOTH directions:
// a new problem fails a test, and so does a fixed problem that is still listed here.
public static class KnownFindings
{
    public sealed record Miss(string Rule, string RecipeId, string Word);

    // F2: unsafe recipes the filter keeps even when the full ingredient list is available.
    // "Word" is the ingredient the keyword lists do not recognise.
    public static readonly IReadOnlyList<Miss> MissedWithFullIngredients = new List<Miss>
    {
        new("allergy: nuts", "r21", "basil pesto (pine nuts; a dish name, not an ingredient word)"),
        new("allergy: eggs", "r09", "Caesar dressing (egg yolk; a composite ingredient)"),
        new("allergy: shellfish", "r24", "mussels, king prawns, squid (plurals; squid not listed)"),
        new("allergy: shellfish", "r46", "mussels (plural; only \"mussel\" is listed)"),
        new("allergy: gluten", "r09", "croutons (plural; only \"crouton\" is listed)"),
        new("allergy: gluten", "r12", "linguine (pasta shape not listed)"),
        new("allergy: gluten", "r50", "bagel (not listed)"),
        new("diet: gluten-free", "r09", "croutons"),
        new("diet: gluten-free", "r12", "linguine"),
        new("diet: gluten-free", "r50", "bagel"),
    };

    // Explanation mismatches found by ExplanationPropertyTests (finding F4).
    // Each kind is described in docs/architecture.md ("Known issues", F4).
    public static readonly IReadOnlySet<string> ExplanationMismatchKinds = new HashSet<string>
    {
        "diet reason shown although keto rule lowered the score",
        "reason not used by the scorer (\"You usually cook this for ...\")",
        "\"recently\" claimed for a view older than 7 days",
        "history reason shown although decayed history lowers the score",
        "strongest factor not mentioned",
    };
}
