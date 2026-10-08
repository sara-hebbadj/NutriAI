# NutriAI offline evaluation report — WITH proposed fix 0001 applied (not applied in the app code)

Generated (UTC): 2026-10-08 13:03. Data: 52 synthetic labelled recipes, 30 synthetic users (seeded), 4 meal times.
Code under test: Services/Recommendation/RecipeScorer.cs, RecommendationService.cs and RecommendationExplainer.cs.
Produced by: dotnet test tests/NutriAI.Tests (EvaluationTests). Deterministic: same numbers on every run.

## 1. Hard-rule filter audit (each rule alone, 52 recipes)

### Full ingredient lists

| Rule | Unsafe recipes | Missed (unsafe but kept) | Safe recipes | Wrongly excluded |
|---|---|---|---|---|
| allergy: nuts | 7 | 0  | 45 | 0  |
| allergy: dairy | 23 | 0  | 29 | 0  |
| allergy: eggs | 13 | 0  | 39 | 0  |
| allergy: shellfish | 7 | 0  | 45 | 0  |
| allergy: gluten | 22 | 0  | 30 | 2 (r20, r43) |
| diet: vegetarian | 22 | 0  | 30 | 0  |
| diet: vegan | 38 | 0  | 14 | 0  |
| diet: halal | 3 | 0  | 49 | 0  |
| diet: gluten-free | 22 | 0  | 30 | 2 (r20, r43) |
| diet: dairy-free | 23 | 0  | 29 | 0  |
| **All rules** | 180 | **0** (0/180 = 0.0%) | 340 | **4** (4/340 = 1.2%) |

### Title only (home feed data)

| Rule | Unsafe recipes | Missed (unsafe but kept) | Safe recipes | Wrongly excluded |
|---|---|---|---|---|
| allergy: nuts | 7 | 0  | 45 | 45 (r01, r02, r03, r04, r06, r08, r09, r10, r11, r12, r13, r14, r15, r16, r17, r18, r19, r20, r22, r23, r24, r25, r26, r27, r28, r29, r30, r31, r32, r35, r37, r38, r39, r40, r41, r42, r43, r44, r45, r46, r47, r48, r49, r50, r52) |
| allergy: dairy | 23 | 0  | 29 | 29 (r03, r04, r07, r08, r10, r13, r14, r15, r16, r18, r20, r22, r24, r28, r30, r31, r32, r33, r34, r35, r37, r39, r40, r43, r44, r46, r48, r49, r52) |
| allergy: eggs | 13 | 0  | 39 | 39 (r01, r05, r07, r08, r10, r11, r12, r13, r14, r15, r16, r17, r18, r20, r21, r23, r24, r25, r26, r27, r28, r29, r33, r34, r36, r37, r38, r40, r41, r42, r43, r44, r45, r46, r47, r48, r49, r50, r52) |
| allergy: shellfish | 7 | 0  | 45 | 45 (r01, r02, r03, r04, r05, r06, r07, r08, r09, r10, r11, r15, r16, r17, r18, r19, r20, r21, r22, r25, r26, r27, r28, r29, r30, r31, r32, r33, r34, r35, r36, r37, r38, r39, r40, r41, r42, r43, r44, r47, r48, r49, r50, r51, r52) |
| allergy: gluten | 22 | 0  | 30 | 30 (r01, r03, r05, r06, r07, r10, r13, r15, r16, r20, r22, r23, r24, r26, r27, r28, r29, r33, r34, r35, r37, r38, r40, r42, r43, r44, r45, r48, r49, r52) |
| diet: vegetarian | 22 | 3 (r11, r32, r42) | 30 | 0  |
| diet: vegan | 38 | 10 (r01, r03, r11, r17, r21, r27, r38, r42, r47, r51) | 14 | 0  |
| diet: halal | 3 | 1 (r39) | 49 | 0  |
| diet: gluten-free | 22 | 13 (r02, r04, r08, r09, r14, r17, r19, r25, r30, r39, r41, r46, r51) | 30 | 0  |
| diet: dairy-free | 23 | 16 (r01, r09, r11, r17, r19, r21, r23, r26, r27, r38, r41, r42, r45, r47, r50, r51) | 29 | 0  |
| **All rules** | 180 | **43** (43/180 = 23.9%) | 340 | **188** (188/340 = 55.3%) |

## 2. Thirty users x four meal times = 120 recommendation lists

| Measure | Full ingredient lists | Title only (home feed) |
|---|---|---|
| Lists where every recommended recipe is safe | 120/120 = 100.0% | 100/120 = 83.3% |
| Lists whose top 5 are all safe | 120/120 = 100.0% | 106/120 = 88.3% |
| Unsafe recipes among all recommended | 0/3260 = 0.0% | 172/780 = 22.1% |
| Unsafe recipes among top-5 slots | 0/600 = 0.0% | 34/100 = 34.0% |
| Safe recipes wrongly excluded (per user) | 20/835 = 2.4% | 683/835 = 81.8% |
| Top-5 diversity: mean distinct cuisines (of 5 cuisine groups; non-empty lists) | 3.28 | 3.15 |
| Top-5 coverage: recipes appearing in any top 5 | 45 / 52 | 23 / 52 |
| Lists with fewer than 5 recipes | 0 / 120 | 100 / 120 |
| Goal effect: mean kcal / protein (g) of top-5 recipes, "lose weight" users | 324 kcal / 15.5 g | n/a (no recipes shown) |
| Goal effect: mean kcal / protein (g) of top-5 recipes, "maintain" users | 398 kcal / 18.4 g | n/a (no recipes shown) |
| Goal effect: mean kcal / protein (g) of top-5 recipes, "gain muscle" users | 439 kcal / 24.3 g | 491 kcal / 27.1 g |

Catalogue average: 437 kcal / 21.7 g protein.

Unsafe recipes shown, full ingredient lists: 

Unsafe recipes shown, title only: r01 (Dairy), r02 (Gluten), r03 (Egg), r04 (Gluten), r08 (Gluten), r09 (Dairy), r09 (Gluten), r11 (Dairy, Meat), r11 (Dairy), r11 (Meat), r14 (Gluten), r17 (Dairy), r17 (Gluten), r19 (Dairy), r19 (Gluten), r21 (Dairy), r23 (Dairy), r25 (Gluten), r26 (Dairy), r27 (Dairy), r30 (Gluten), r32 (Meat), r38 (Dairy), r39 (Gluten), r39 (Pork), r41 (Dairy), r41 (Gluten), r42 (Dairy, Meat), r42 (Dairy), r42 (Meat), r45 (Dairy), r46 (Gluten), r47 (Dairy), r50 (Dairy), r51 (Dairy, Egg), r51 (Dairy), r51 (Gluten)
