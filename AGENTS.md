# Notes for coding agents working on NutriAI

NutriAI is Sara Hebbadj's MSc dissertation app (University of Hull, 2026): .NET 9 MAUI, C#, MVVM, SQLite, Spoonacular API. This is the portfolio version. Sara must be able to explain every line in an interview, so keep changes small, named plainly and commented where a decision is not obvious.

## Layout

| Path | What it is |
|---|---|
| `NutriAI.csproj`, `NutriAI.sln` | The MAUI app (Android, iOS, Windows) |
| `Models/`, `DTOs/`, `Preprocessing/` | Data shapes and feature normalisation |
| `Services/Recommendation/` | The engine: `RecipeScorer` (filter + score), `RecommendationService` (rank), `RecommendationExplainer` (reasons), `TimeDecay`, `ContextHelper` |
| `Services/` (other) | Spoonacular client, cache, JSON stores, SQLite interactions, `SpoonacularSettings` (API key from the environment) |
| `ViewModels/`, `Views/`, `Platforms/`, `Resources/` | MAUI UI |
| `tests/NutriAI.Tests/` | xUnit tests (plain `net9.0`), synthetic data in `TestData/`, helpers in `Helpers/` |
| `evals/` | Saved evaluation reports (dated) |
| `proposed-fixes/` | Patches suggested but **not applied**, with their own README |
| `docs/architecture.md` | Diagrams, ranking sequence and the known issues found by the tests |
| `Documentation/` | Sara's dissertation documents: authorship statement, guides, screenshots |

## Commands

```bash
dotnet test tests/NutriAI.Tests/NutriAI.Tests.csproj          # works on Windows, macOS, Linux; no key needed
dotnet build -t:Run -f net9.0-windows10.0.19041.0              # Windows only, needs the MAUI workload
dotnet build -f net9.0-android -p:TargetFrameworks=net9.0-android   # Android, needs the Android SDK
```

Set `NUTRIAI_EVAL_REPORT=<path>` to save the evaluation report from `EvaluationTests` to a file.

## Rules

1. **Secrets.** Never put an API key in code, tests, docs or commit messages. The key comes from `SPOONACULAR_API_KEY` (or the git-ignored `Platforms/Android/spoonacular.env`). `ConfigurationTests` fails on any 32-character hex string in a `.cs` file. Never print key values, even from old commits.
2. **Do not change the scoring logic without Sara.** `RecipeScorer`, `RecommendationService` and `RecommendationExplainer` are her dissertation design. Fixes go to `proposed-fixes/` as a patch with a README until she approves them.
3. **If the formula changes,** update `tests/NutriAI.Tests/Helpers/ScoreFactors.cs` (it mirrors the formula; `ScoreFactors_helper_matches_the_real_scorer` checks it) and the README formula table.
4. **Known findings are a two-way ledger.** `KnownFindings.cs` lists every filter miss and explanation-mismatch kind the tests found. `EvaluationTests` and `ExplanationPropertyTests` fail if the real behaviour differs from the ledger in either direction. When you fix something, remove it from the ledger, flip its test in `KnownFindingsTests.cs`, and update `docs/architecture.md`. Never add an entry just to make a test pass without documenting why.
5. **Test project compiles app files by link.** Only MAUI-free files can be linked (no `FileSystem`, `Shell`, `Command`, XAML). If you link a new file, check it builds on Linux.
6. **Data.** Test data is synthetic and must stay synthetic. Do not commit Spoonacular responses, cached recipes or any `.db` file.
7. **Honesty.** Numbers in README or RESULTS come only from a saved run in `evals/` (or `proposed-fixes/` for patched runs), with denominators and dates. Do not claim allergy safety.
8. **Keep authorship notes.** The comment block at the top of each of Sara's files records how it was written (including where Copilot helped). Do not remove or reword them; mark new changes "Portfolio change (October 2026)".
9. **Publishing** (creating or pushing a GitHub repo, renaming, making public) needs Sara's explicit OK each time.
