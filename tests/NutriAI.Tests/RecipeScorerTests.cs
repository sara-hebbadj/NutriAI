using NutriAI.Services.Recommendation;
using NutriAI.Tests.Helpers;

namespace NutriAI.Tests;

// Unit tests for Services/Recommendation/RecipeScorer.cs, one rule at a time.
// Each test keeps every other factor neutral so the expected number can be worked out by hand.
public class RecipeScorerTests
{
    private const double Tolerance = 1e-4;
    private static readonly MealContext NoMatch = MealContext.Snack; // Make.Recipe has no meal type

    [Fact]
    public void Never_seen_recipe_gets_the_cold_start_score()
    {
        var r = Make.Recipe();
        var score = RecipeScorer.Score(r, Make.Features(), Make.NoHistory(r), NoMatch, Make.Prefs());

        // 0.5 baseline x 0.3 weight for "no interaction yet"
        Assert.Equal(0.15, score, Tolerance);
    }

    [Theory]
    [InlineData(1, 0, 0, 0.6)] // a view adds 0.1
    [InlineData(0, 1, 0, 1.1)] // a save adds 0.6
    [InlineData(0, 0, 1, 1.7)] // a cook adds 1.2
    [InlineData(2, 1, 1, 2.5)] // 0.5 + 0.2 + 0.6 + 1.2
    public void Interaction_weights_are_view_0_1_save_0_6_cook_1_2(int views, int saves, int cooks, double behaviour)
    {
        var r = Make.Recipe();
        var history = Make.History(r, views, saves, cooks); // no timestamps -> recency weight 0.3

        var score = RecipeScorer.Score(r, Make.Features(), history, NoMatch, Make.Prefs());

        Assert.Equal(behaviour * 0.3, score, Tolerance);
    }

    [Fact]
    public void Interaction_weight_halves_every_seven_days()
    {
        var r = Make.Recipe();
        var today = RecipeScorer.Score(r, Make.Features(), Make.History(r, views: 1, viewedDaysAgo: 0), NoMatch, Make.Prefs());
        var weekAgo = RecipeScorer.Score(r, Make.Features(), Make.History(r, views: 1, viewedDaysAgo: 7), NoMatch, Make.Prefs());

        Assert.Equal(0.6, today, Tolerance);
        Assert.Equal(0.3, weekAgo, Tolerance);
    }

    [Fact]
    public void Recency_uses_the_last_cook_date_before_the_last_view_date()
    {
        var r = Make.Recipe();
        var history = Make.History(r, views: 1, cooks: 1, viewedDaysAgo: 0, cookedDaysAgo: 14);

        var score = RecipeScorer.Score(r, Make.Features(), history, NoMatch, Make.Prefs());

        // (0.5 + 0.1 + 1.2) x 0.25 (two half-lives), even though the recipe was viewed today
        Assert.Equal(1.8 * 0.25, score, Tolerance);
    }

    [Theory]
    [InlineData("breakfast", MealContext.Breakfast, 1.25)]
    [InlineData("breakfast", MealContext.Dinner, 1.0)]
    [InlineData("meal", MealContext.Lunch, 1.25)]   // "meal" (main course, side, soup) fits lunch and dinner
    [InlineData("meal", MealContext.Dinner, 1.25)]
    [InlineData("meal", MealContext.Breakfast, 1.0)]
    [InlineData("", MealContext.Lunch, 1.0)]
    public void Meal_context_boost_is_1_25_for_a_matching_meal(string mealType, MealContext context, double boost)
    {
        var r = Make.Recipe(meal: mealType);
        var score = RecipeScorer.Score(r, Make.Features(), Make.NoHistory(r), context, Make.Prefs());

        Assert.Equal(0.15 * boost, score, Tolerance);
    }

    [Theory]
    [InlineData("halal", "Bacon Pasta", "200 g bacon")]
    [InlineData("vegan", "Cheese Toastie", "2 slices cheddar")]
    [InlineData("vegetarian", "Chicken Wrap", "1 chicken breast")]
    [InlineData("vegetarian", "Tuna Melt", "1 can tuna")]
    [InlineData("gluten-free", "Toast", "2 slices bread")]
    [InlineData("dairy-free", "Garlic Rice", "1 tbsp butter")]
    public void Dietary_rule_violation_scores_zero(string diet, string title, string ingredient)
    {
        var r = Make.Recipe(title, ingredients: ingredient);
        var prefs = Make.Prefs(diets: new[] { diet });

        Assert.Equal(0.0, RecipeScorer.Score(r, Make.Features(), Make.NoHistory(r), NoMatch, prefs));
    }

    [Theory]
    [InlineData("nuts", "2 tbsp peanut butter")]
    [InlineData("nuts", "50 g almonds")]
    [InlineData("dairy", "1 cup milk")]
    [InlineData("eggs", "2 large eggs")]
    [InlineData("shellfish", "300 g shrimp")]
    [InlineData("gluten", "2 cups all-purpose flour")]
    public void Allergen_in_ingredients_scores_zero(string allergy, string ingredient)
    {
        var r = Make.Recipe("Mystery Dish", ingredients: ingredient);
        var prefs = Make.Prefs(allergies: new[] { allergy });

        Assert.Equal(0.0, RecipeScorer.Score(r, Make.Features(), Make.NoHistory(r), NoMatch, prefs));
    }

    [Fact]
    public void Allergen_in_title_scores_zero_even_with_strong_history()
    {
        var r = Make.Recipe("PEANUT-Butter Cookies"); // case and hyphen are normalised
        var loved = Make.History(r, views: 10, saves: 3, cooks: 5, viewedDaysAgo: 0, cookedDaysAgo: 0);

        Assert.Equal(0.0, RecipeScorer.Score(r, Make.Features(), loved, NoMatch, Make.Prefs(allergies: new[] { "nuts" })));
    }

    [Theory]
    [InlineData("eggs", "1 eggplant")]          // "egg" must be a whole word
    [InlineData("nuts", "1/4 tsp nutmeg")]
    [InlineData("nuts", "1 cup coconut flakes")]
    public void Keyword_must_be_a_whole_word(string allergy, string ingredient)
    {
        var r = Make.Recipe("Mystery Dish", ingredients: ingredient);
        var prefs = Make.Prefs(allergies: new[] { allergy });

        Assert.True(RecipeScorer.Score(r, Make.Features(), Make.NoHistory(r), NoMatch, prefs) > 0);
    }

    [Theory]
    [InlineData(20, 1.3)]
    [InlineData(21, 0.4)]
    public void Keto_rewards_20g_carbs_or_less_and_penalises_more(int carbs, double weight)
    {
        var r = Make.Recipe(carbs: carbs);
        var score = RecipeScorer.Score(r, Make.Features(), Make.NoHistory(r), NoMatch, Make.Prefs(diets: new[] { "keto" }));

        Assert.Equal(0.15 * weight, score, Tolerance);
    }

    [Theory]
    [InlineData("lose weight", 1800, 500, 15, 1.25)] // within the daily target
    [InlineData("lose weight", 400, 500, 15, 0.8)]   // above the target
    [InlineData("lose weight", null, 500, 15, 0.8)]  // no target set
    [InlineData("gain muscle", null, 500, 25, 1.3)]  // 25 g protein or more
    [InlineData("gain muscle", null, 500, 24, 0.9)]
    [InlineData("maintain", null, 500, 15, 1.0)]
    public void Health_goal_weight(string goal, int? target, int kcal, int protein, double weight)
    {
        var r = Make.Recipe(kcal: kcal, protein: protein);
        var prefs = Make.Prefs(goal: goal, calorieTarget: target);

        // Features stay 0 here, so for "lose weight" the feature weight is 1.2 (1.2 - 0 x 0.5).
        var featureWeight = goal == "lose weight" ? 1.2 : 1.0;
        var score = RecipeScorer.Score(r, Make.Features(), Make.NoHistory(r), NoMatch, prefs);

        Assert.Equal(0.15 * featureWeight * weight, score, Tolerance);
    }

    [Theory]
    [InlineData("gain muscle", 0f, 1f, 0f, 1.5 * 1.3)]   // highest protein in the set: x1.5
    [InlineData("lose weight", 1f, 0f, 0f, 0.7 * 1.25)]  // highest calories in the set: x0.7
    [InlineData("maintain", 0f, 0f, 0.81f, 0.9)]         // among the longest cooking times: x0.9
    public void Normalised_features_shift_the_score(string goal, float calories, float protein, float time, double weight)
    {
        var r = Make.Recipe(kcal: 500, protein: 30);
        var prefs = Make.Prefs(goal: goal, calorieTarget: 1800);
        var features = Make.Features(calories, protein, time);

        var score = RecipeScorer.Score(r, features, Make.NoHistory(r), NoMatch, prefs);

        Assert.Equal(0.15 * weight, score, Tolerance);
    }
}
