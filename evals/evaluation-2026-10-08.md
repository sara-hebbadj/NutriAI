# NutriAI offline evaluation report

Generated (UTC): 2026-10-08 13:04. Data: 52 synthetic labelled recipes, 30 synthetic users (seeded), 4 meal times.
Code under test: Services/Recommendation/RecipeScorer.cs, RecommendationService.cs and RecommendationExplainer.cs.
Produced by: dotnet test tests/NutriAI.Tests (EvaluationTests). Deterministic: same numbers on every run.

## 1. Hard-rule filter audit (each rule alone, 52 recipes)

### Full ingredient lists

| Rule | Unsafe recipes | Missed (unsafe but kept) | Safe recipes | Wrongly excluded |
|---|---|---|---|---|
| allergy: nuts | 7 | 1 (r21) | 45 | 12 (r01, r02, r06, r12, r13, r16, r19, r27, r37, r41, r45, r49) |
| allergy: dairy | 23 | 0  | 29 | 7 (r07, r13, r16, r33, r37, r48, r49) |
| allergy: eggs | 13 | 1 (r09) | 39 | 0  |
| allergy: shellfish | 7 | 2 (r24, r46) | 45 | 0  |
| allergy: gluten | 22 | 3 (r09, r12, r50) | 30 | 5 (r13, r20, r26, r37, r43) |
| diet: vegetarian | 22 | 0  | 30 | 10 (r03, r10, r17, r20, r27, r30, r40, r43, r47, r48) |
| diet: vegan | 38 | 0  | 14 | 10 (r07, r10, r16, r20, r33, r37, r40, r43, r48, r49) |
| diet: halal | 3 | 0  | 49 | 0  |
| diet: gluten-free | 22 | 3 (r09, r12, r50) | 30 | 5 (r13, r20, r26, r37, r43) |
| diet: dairy-free | 23 | 0  | 29 | 7 (r07, r13, r16, r33, r37, r48, r49) |
| **All rules** | 180 | **10** (10/180 = 5.6%) | 340 | **56** (56/340 = 16.5%) |

### Title only (home feed data)

| Rule | Unsafe recipes | Missed (unsafe but kept) | Safe recipes | Wrongly excluded |
|---|---|---|---|---|
| allergy: nuts | 7 | 2 (r21, r36) | 45 | 1 (r12) |
| allergy: dairy | 23 | 17 (r01, r02, r09, r11, r17, r19, r21, r23, r26, r27, r38, r41, r42, r45, r47, r50, r51) | 29 | 2 (r07, r33) |
| allergy: eggs | 13 | 8 (r02, r03, r06, r09, r22, r31, r39, r51) | 39 | 0  |
| allergy: shellfish | 7 | 3 (r14, r24, r46) | 45 | 0  |
| allergy: gluten | 22 | 17 (r02, r04, r08, r09, r12, r14, r17, r18, r19, r30, r36, r39, r41, r46, r47, r50, r51) | 30 | 0  |
| diet: vegetarian | 22 | 3 (r11, r32, r42) | 30 | 0  |
| diet: vegan | 38 | 11 (r01, r02, r03, r11, r17, r21, r27, r38, r42, r47, r51) | 14 | 2 (r07, r33) |
| diet: halal | 3 | 1 (r39) | 49 | 0  |
| diet: gluten-free | 22 | 17 (r02, r04, r08, r09, r12, r14, r17, r18, r19, r30, r36, r39, r41, r46, r47, r50, r51) | 30 | 0  |
| diet: dairy-free | 23 | 17 (r01, r02, r09, r11, r17, r19, r21, r23, r26, r27, r38, r41, r42, r45, r47, r50, r51) | 29 | 2 (r07, r33) |
| **All rules** | 180 | **96** (96/180 = 53.3%) | 340 | **7** (7/340 = 2.1%) |

## 2. Thirty users x four meal times = 120 recommendation lists

| Measure | Full ingredient lists | Title only (home feed) |
|---|---|---|
| Lists where every recommended recipe is safe | 56/120 = 46.7% | 0/120 = 0.0% |
| Lists whose top 5 are all safe | 103/120 = 85.8% | 29/120 = 24.2% |
| Unsafe recipes among all recommended | 136/2576 = 5.3% | 1360/4592 = 29.6% |
| Unsafe recipes among top-5 slots | 18/580 = 3.1% | 192/600 = 32.0% |
| Safe recipes wrongly excluded (per user) | 225/835 = 26.9% | 27/835 = 3.2% |
| Top-5 diversity: mean distinct cuisines (of 5 cuisine groups; non-empty lists) | 2.98 | 3.29 |
| Top-5 coverage: recipes appearing in any top 5 | 48 / 52 | 49 / 52 |
| Lists with fewer than 5 recipes | 16 / 120 | 0 / 120 |
| Goal effect: mean kcal / protein (g) of top-5 recipes, "lose weight" users | 347 kcal / 17.3 g | 367 kcal / 18.9 g |
| Goal effect: mean kcal / protein (g) of top-5 recipes, "maintain" users | 395 kcal / 19.0 g | 427 kcal / 19.1 g |
| Goal effect: mean kcal / protein (g) of top-5 recipes, "gain muscle" users | 412 kcal / 22.8 g | 489 kcal / 27.2 g |

Catalogue average: 437 kcal / 21.7 g protein.

Unsafe recipes shown, full ingredient lists: r09 (Egg, Gluten), r09 (Egg), r09 (Gluten), r12 (Gluten), r21 (Nuts), r24 (Shellfish), r46 (Shellfish), r50 (Gluten)

Unsafe recipes shown, title only: r01 (Dairy), r02 (Dairy, Egg, Gluten), r02 (Dairy, Egg), r02 (Dairy, Gluten), r02 (Dairy), r02 (Egg, Gluten), r02 (Egg), r02 (Gluten), r03 (Egg), r04 (Gluten), r06 (Egg), r08 (Gluten), r09 (Dairy, Egg, Gluten), r09 (Dairy, Gluten), r09 (Dairy), r09 (Egg, Gluten), r09 (Egg), r09 (Gluten), r11 (Dairy, Meat), r11 (Dairy), r11 (Meat), r12 (Gluten), r14 (Gluten), r14 (Shellfish, Gluten), r14 (Shellfish), r17 (Dairy, Gluten), r17 (Dairy), r17 (Gluten), r18 (Gluten), r19 (Dairy, Gluten), r19 (Dairy), r19 (Gluten), r21 (Dairy), r21 (Nuts), r22 (Egg), r23 (Dairy), r24 (Shellfish), r26 (Dairy), r27 (Dairy), r30 (Gluten), r31 (Egg), r32 (Meat), r36 (Gluten), r36 (Nuts, Gluten), r36 (Nuts), r38 (Dairy), r39 (Egg, Gluten), r39 (Egg), r39 (Gluten, Pork), r39 (Gluten), r39 (Pork), r41 (Dairy, Gluten), r41 (Dairy), r41 (Gluten), r42 (Dairy, Meat), r42 (Dairy), r42 (Meat), r45 (Dairy), r46 (Gluten), r46 (Shellfish, Gluten), r46 (Shellfish), r47 (Dairy, Gluten), r47 (Dairy), r47 (Gluten), r50 (Dairy, Gluten), r50 (Dairy), r50 (Gluten), r51 (Dairy, Egg, Gluten), r51 (Dairy, Egg), r51 (Dairy, Gluten), r51 (Dairy), r51 (Egg, Gluten), r51 (Egg), r51 (Gluten)
