# Proposed fixes (not applied)

These patches are suggestions for Sara to review. The app code in this repository is still the dissertation version, so the tests and README describe what the dissertation code actually does. Apply a patch only after reading it and deciding it is right.

## 0001-safer-allergen-filter.patch

Written by the coding agent on 8 October 2026 for findings F1–F4 (see [docs/architecture.md → Known issues](../docs/architecture.md#known-issues-found-by-the-tests)).

| Part | File | Change | Checked how |
|---|---|---|---|
| Phrase matching (F3) | `Services/Recommendation/RecipeScorer.cs` | `ContainsAny` matches each keyword as a whole phrase inside one ingredient line or the title, so "fish sauce" no longer matches every "sauce" | Unit tests + evaluation |
| Plurals (F2) | same | "prawn" also matches "prawns", "anchovy" matches "anchovies" | Unit tests + evaluation |
| Missing words (F2) | same | Adds pasta and bread names, more shellfish, dairy and nut words, and egg-based sauces (Caesar dressing, hollandaise, carbonara); adds "not this allergen" phrases such as "coconut milk", "peanut butter" (for dairy) and "vegetable stock" | Unit tests + evaluation |
| Unknown ingredients (F1) | same | A user with any allergy is not shown a recipe whose ingredient list is empty | Unit tests + evaluation |
| Feed ingredients (F1) | `DTOs/NutritionDtos.cs`, `Services/ApiRecipeService.cs` | Keeps ingredient names from the search response (`nutrition.ingredients[].name`, returned with `addRecipeNutrition=true`) | Compiles only. **Not checked against a live Spoonacular response** |
| Honest reasons (F4) | `Services/Recommendation/RecommendationExplainer.cs` | No "Fits your dietary preferences" when the keto rule lowered the score; "You viewed this recipe before" instead of "You viewed similar recipes recently" | Unit tests + explanation check |
| Tests | `tests/NutriAI.Tests/*` | Known-finding tests flipped to the fixed behaviour; ledger emptied | `dotnet test`: 85 passed |

### Apply and test

```powershell
git apply proposed-fixes/0001-safer-allergen-filter.patch
dotnet test tests/NutriAI.Tests/NutriAI.Tests.csproj
```

### Measured with the patch applied (8 October 2026, same synthetic data)

Full report: [evaluation-with-fix-2026-10-08.md](evaluation-with-fix-2026-10-08.md).

| Measure | Dissertation code | With patch 0001 |
|---|---|---|
| Unsafe recipe–rule pairs missed (full ingredient lists) | 10 / 180 | 0 / 180 |
| Safe recipe–rule pairs wrongly excluded (full ingredient lists) | 56 / 340 | 4 / 340 |
| User lists where every recipe is safe (full ingredient lists) | 56 / 120 | 120 / 120 |
| Safe recipes wrongly excluded per user (full ingredient lists) | 225 / 835 | 20 / 835 |
| Lists with fewer than 5 recipes (full ingredient lists) | 16 / 120 | 0 / 120 |
| User lists where every recipe is safe (title only) | 0 / 120 | 100 / 120, but all 100 are **empty** lists of users with allergies; the 20 lists of the 5 diet-only users still contain unsafe recipes |
| Explanation mismatch kinds (5,000 cases) | 5 | 3 |

### Read these numbers with care

- **Not independent.** The patch was written after looking at the same 52 recipes, so 0 misses here is expected and proves little. Before trusting it, write 20–30 new recipes (ideally someone who has not read the keyword lists) and run the same audit.
- **Fail-closed hides the feed.** Without ingredient data, users with allergies would see an empty home page. The feed mapping part of the patch should prevent that, but only a live API call can confirm the search response really contains `nutrition.ingredients`. Check it with your own key before applying.
- **Dietary rules are not fail-closed.** The patch only hides unknown-ingredient recipes from users with allergies; a vegetarian or halal user can still see a title-only recipe that breaks their rule. Whether to extend it is a product decision.
- Two safe recipes are still excluded for gluten: chickpea flour (matches "flour") and tamari described as "gluten-free soy sauce" (matches "soy sauce"). Both are the cautious direction.
- Not covered: F5 (calorie goal vs daily target), F6 (empty list), F7 (decay below cold start), F8 (search page) and the remaining explanation mismatches ("You usually cook this for …", decayed history, strongest factor not named).
