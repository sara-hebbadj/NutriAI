# LEARN: NutriAI in 10 minutes

A script for walking an interviewer through the project, ten likely questions with short answers, and three changes to practise live. Everything here matches the code in this repository.

## 10-minute walkthrough

**0:00 — The problem (1 min).** "People rarely rate recipes. NutriAI learns from what they do instead: open, save, cook. It must never break an allergy or dietary rule, and it explains each suggestion." Show `Documentation/Screenshots/homepage.png` and `profileoptions.jpg`.

**1:00 — Architecture (1.5 min).** Open the Mermaid diagram in `README.md`. MVVM: `Views/HomePage.xaml` binds to `ViewModels/HomeViewModel.cs`, which calls `ApiRecipeService` (Spoonacular + JSON cache) and `RecommendationService`. Interactions live in SQLite (`SQLiteUserInteractionService`), preferences in a JSON file. The engine (`Services/Recommendation/`) is plain C#.

**2:30 — The scoring formula (2.5 min).** Open `Services/Recommendation/RecipeScorer.cs`.
- Lines 27–32: hard rules first. Any allergy or diet violation returns 0, and `RecommendationService.Rank` drops scores of 0.
- Lines 36–40: behaviour `0.5 + 0.1·views + 0.6·saves + 1.2·cooks`.
- Lines 43–44 and `TimeDecay.cs`: 7-day half-life; 0.3 if the recipe was never touched.
- Lines 47–62: ×1.25 if the meal type fits the time of day (`ContextHelper.cs`).
- Lines 65–77: goal and keto weights, then everything is multiplied.
- Worked example: cooked once a week ago at lunch time = `1.8 × 0.5 × 1.25 = 1.125`; never-seen = `0.15`.

**5:00 — Explanations (1 min).** `RecommendationExplainer.cs`: up to three reasons from the same inputs. Show `cooked3.png` ("Why this recipe was recommended").

**6:00 — Testing it (2 min).** `tests/NutriAI.Tests` is a plain .NET 9 xUnit project that compiles the app's own scoring files by link, so it runs on any OS and in GitHub Actions. Run `dotnet test`: 85 tests. Show one unit test (`RecipeScorerTests.Interaction_weights_are_view_0_1_save_0_6_cook_1_2`) and one property test (`GoalAndSafetyPropertyTests.A_recipe_the_filter_excludes_never_appears_however_much_the_user_liked_it`).

**8:00 — What the tests found (1.5 min).** `evals/evaluation-2026-10-08.md`: with full ingredient lists the filter missed 10 of 180 unsafe recipe–rule pairs (plurals like "mussels", dish names like "pesto") and wrongly excluded 56 of 340 safe ones ("fish sauce" made every "sauce" non-vegetarian). Worse, the home feed has only titles (F1), so 0 of 120 user lists were fully safe there. These are documented in `docs/architecture.md` with a proposed patch that is not applied, because the patch was tuned on the same data.

**9:30 — Close (0.5 min).** "The rules are explainable and testable; the tests showed that keyword matching is not enough for allergy safety, and what I would change."

## Ten interview questions

**1. Walk me through the scoring formula. Why these weights?**
Filter first: any allergy or diet violation scores 0 and is removed. Then `score = behaviour × recency × mealTime × featureWeight × ketoWeight × goalWeight`. Behaviour is `0.5 + 0.1·views + 0.6·saves + 1.2·cooks`: the weights rise with commitment (looking < intending < doing), and 0.5 gives new recipes a chance. Recency halves every 7 days. The weights are hand-set design choices, not learned; the tests check their direction (cooked > saved > viewed > unseen), not that they are optimal. With real users I would tune them on held-out interaction data.

**2. How do you guarantee an allergen never appears?**
I can guarantee one thing and not another. Guaranteed by design and tested: once the filter flags a recipe, nothing can bring it back; history cannot outweigh it, because the score is set to 0 before ranking and 0 is removed (`A_recipe_the_filter_excludes_never_appears...`). Not guaranteed: that the filter flags every allergen. It matches keywords, and the tests found misses (plurals, "pesto", "Caesar dressing") and that home-feed recipes have no ingredient list at all. So I would not claim allergy safety. The fix is better data (ingredient lists in the feed, Spoonacular's `intolerances` filter) and failing closed: if ingredients are unknown, do not show the recipe to a user with allergies.

**3. Why a rule-based scorer instead of a trained model?**
A new user has no history and there were no labels to train on. Rules work from the first launch, every decision can be explained, and the hard safety step is separate from the soft ranking, so it can be tested on its own. The trade-off is that the weights are guesses and it does not learn from other users.

**4. What is implicit feedback, and what are its weaknesses?**
Signals from behaviour (views, saves, cooks) instead of ratings. There are no negative signals (a view can mean "not for me"), and it is biased towards what was shown first. Here a view is weak (0.1), a cook strong (1.2).

**5. How does recency work, and is there a problem with it?**
`TimeDecay.Compute` returns `0.5^(days/7)`. A recipe never touched gets 0.3. One view more than 14 days old decays below that, so an old interest ranks lower than something never seen (finding F7). It might be intended forgetting, but it should be a decision.

**6. How did you test that explanations match the score?**
`Helpers/ScoreFactors.cs` splits the score into its six parts; a test checks it equals the real `RecipeScorer.Score` on 3,000 random inputs. Then 5,000 random cases compare the reasons with the parts. Every reason was true, but some mismatched: "Fits your dietary preferences" when keto cut the score to 40%, "recently" for month-old views, and "You usually cook this for lunch", which the score never uses.

**7. Tell me about one bug and how you found it.**
Over-exclusion: `ContainsAny` splits "fish sauce" into "fish" and "sauce" and matches either, so a vegetarian never sees anything with "tomato sauce", and "ground beef" excludes "freshly ground black pepper". I found it by labelling 52 recipes by hand and auditing each rule: 56 of 340 safe pairs were excluded. The fix is to match whole phrases.

**8. How did you handle the API key?**
The dissertation code had it as a constant, and the zip is public, so the key is exposed and must be regenerated. Now `SpoonacularSettings.GetApiKey()` reads the `SPOONACULAR_API_KEY` environment variable; on Android a git-ignored file feeds it in. A test fails if any 32-character hex string appears in the source.

**9. How would you make it learn from more users?**
Move interactions to a server with consent, then add collaborative filtering (users who cooked X also cooked Y, or matrix factorisation) as one more factor in the product, keeping the hard filter first. Cold start stays rule-based. Evaluate offline with held-out cooks (hit rate, nDCG) before an A/B test, and keep the safety audit as a gate.

**10. Why is the test project separate from the MAUI app?**
MAUI needs platform workloads and cannot build on a Linux CI runner. The scoring files use no MAUI APIs, so a plain `net9.0` xUnit project compiles them by link (`<Compile Include="..\..\Services\Recommendation\*.cs" Link=...>`). The tests run Sara's real code, not a copy, and `NutriAI.csproj` excludes `tests/` so the app never compiles test code.

## Three changes to practise live

Run `dotnet test tests/NutriAI.Tests/NutriAI.Tests.csproj` before and after each one.

**Exercise 1: change a weight (10 minutes).**
In `RecipeScorer.cs` change the cook weight from `1.2` to `2.0`.
Expected: the tests that use the cook weight fail (`Interaction_weights_are_view_0_1_save_0_6_cook_1_2`, `Recency_uses_the_last_cook_date_before_the_last_view_date`, `ScoreFactors_helper_matches_the_real_scorer`). Update the expected numbers in those tests, update the same weight in `tests/NutriAI.Tests/Helpers/ScoreFactors.cs`, and say what changes for users (cooked recipes dominate even longer before decay catches up). Then put it back.

**Exercise 2: fix one plural (10 minutes).**
Add `"mussels"` to `ShellfishKeywords`.
Expected: the evaluation test fails because the ledger in `KnownFindings.cs` still lists the two shellfish misses, r24 (Seafood Paella) and r46 (Mussels in Tomato Broth), which are now caught (one matching word is enough). Remove both lines from `MissedWithFullIngredients`, change the `"200 g mussels"` case in `F2_allergen_word_not_in_the_keyword_list_is_not_excluded` to expect 0, and rerun. Explain why the ledger fails in both directions.

**Exercise 3: make one explanation honest (15 minutes).**
In `RecommendationExplainer.cs`, skip "Fits your dietary preferences" when the user is keto and the recipe has more than 20 g carbs.
Expected: `F4_keto_penalty_is_explained_as_fits_your_dietary_preferences` fails (update it to `Assert.DoesNotContain`), and `Explanation_mismatches_are_only_the_documented_kinds` fails until you remove that kind from `KnownFindings.ExplanationMismatchKinds`. Compare your version with the same change in `proposed-fixes/0001-safer-allergen-filter.patch`.
