using System.Collections.ObjectModel;
using NutriAI.Models;
using NutriAI.Services;
using NutriAI.Services.Interactions;
using NutriAI.Services.Recommendation;
using NutriAI.Services.Storage;
using NutriAI.Tests.Helpers;
using NutriAI.ViewModels;

namespace NutriAI.Tests;

// Each test here reproduces one finding in the smallest possible example and asserts
// what the code does TODAY. They pass now on purpose, so CI stays green while the problem
// stays visible. When a fix from proposed-fixes/ is applied, these tests must be updated
// to assert the corrected behaviour. Details and severity: docs/architecture.md, "Known issues".
public class KnownFindingsTests
{
    private static readonly MealContext Lunch = MealContext.Lunch;

    private static double Score(Recipe r, UserPreferences prefs) =>
        RecipeScorer.Score(r, Make.Features(), Make.NoHistory(r), Lunch, prefs);

    // F1 (high): the home feed only knows recipe titles, so allergens hidden in the
    // ingredients are not seen. ApiRecipeService.LoadRecipesAsync maps title, image, time,
    // nutrition and categories from the search results, but no ingredients.
    [Fact]
    public async Task F1_home_feed_shows_an_allergen_recipe_when_only_the_title_is_known()
    {
        var pancakesTitleOnly = new Recipe { Id = "1", Title = "Fluffy Buttermilk Pancakes", MealType = "breakfast" };
        var home = HomeWith(new[] { pancakesTitleOnly }, Make.Prefs(allergies: new[] { "dairy" }));

        await home.InitializeAsync();

        // Today: shown to a user with a dairy allergy (buttermilk, butter and milk are in the real recipe).
        Assert.Contains(home.FilteredRecipes, r => r.Id == "1");

        // The same recipe with its ingredient list (as loaded on the details page) is excluded.
        var withIngredients = Make.Recipe("Fluffy Buttermilk Pancakes", ingredients: new[] { "2 cups buttermilk", "3 tbsp melted butter" });
        Assert.Equal(0.0, Score(withIngredients, Make.Prefs(allergies: new[] { "dairy" })));
    }

    // F2 (high): allergen words the keyword lists do not contain: plurals and dish names.
    [Theory]
    [InlineData("shellfish", "200 g mussels")]
    [InlineData("shellfish", "300 g large prawns, peeled")]
    [InlineData("shellfish", "300 g sea scallops")]
    [InlineData("gluten", "1/2 cup croutons")]
    [InlineData("gluten", "250 g linguine")]
    [InlineData("gluten", "1 plain bagel")]
    [InlineData("nuts", "1/3 cup basil pesto")]
    [InlineData("eggs", "3 tbsp Caesar dressing")]
    public void F2_allergen_word_not_in_the_keyword_list_is_not_excluded(string allergy, string ingredient)
    {
        var r = Make.Recipe("Dinner", ingredients: ingredient);

        Assert.True(Score(r, Make.Prefs(allergies: new[] { allergy })) > 0); // kept today
    }

    [Fact]
    public void F2_the_singular_form_is_caught_but_the_plural_is_not()
    {
        var shellfish = Make.Prefs(allergies: new[] { "shellfish" });

        Assert.Equal(0.0, Score(Make.Recipe("Dinner", ingredients: "1 prawn"), shellfish));
        Assert.True(Score(Make.Recipe("Dinner", ingredients: "6 prawns"), shellfish) > 0);
    }

    // F3 (medium): over-exclusion. Multi-word keywords are split into single words, so common
    // words such as "sauce" (from "fish sauce"), "ground" (from "ground beef"), "butter"
    // (from "peanut butter") or "powder" (from "milk powder") exclude safe recipes. A few listed
    // single words are also too broad ("stock" and "broth" for meat, "milk" for dairy).
    [Theory]
    [InlineData("vegetarian", "salt and freshly ground black pepper")]
    [InlineData("vegetarian", "1/2 cup tomato sauce")]
    [InlineData("vegetarian", "4 cups vegetable stock")]
    [InlineData("halal", "a dash of hot sauce")]
    [InlineData("gluten-free", "1 cup white rice")]
    public void F3_safe_ingredient_is_excluded_for_a_diet(string diet, string ingredient)
    {
        var r = Make.Recipe("Dinner", ingredients: ingredient);

        Assert.Equal(0.0, Score(r, Make.Prefs(diets: new[] { diet }))); // wrongly excluded today
    }

    [Theory]
    [InlineData("nuts", "2 tbsp butter")]
    [InlineData("nuts", "1 cup milk")]
    [InlineData("dairy", "2 tsp baking powder")]
    [InlineData("dairy", "400 ml coconut milk")]
    [InlineData("gluten", "1 tbsp fish sauce")]
    public void F3_safe_ingredient_is_excluded_for_an_allergy(string allergy, string ingredient)
    {
        var r = Make.Recipe("Dinner", ingredients: ingredient);

        Assert.Equal(0.0, Score(r, Make.Prefs(allergies: new[] { allergy }))); // wrongly excluded today
    }

    // F4 (medium): explanations that do not match the score.
    [Fact]
    public void F4_keto_penalty_is_explained_as_fits_your_dietary_preferences()
    {
        var pasta = Make.Recipe("Pasta", carbs: 80);
        var keto = Make.Prefs(diets: new[] { "keto" });

        var reasons = RecommendationExplainer.Explain(pasta, Make.NoHistory(pasta), Lunch, keto);

        Assert.Contains("Fits your dietary preferences", reasons);  // shown to the user...
        Assert.Equal(0.15 * 0.4, Score(pasta, keto), 4);            // ...although keto cut the score to 40%
    }

    [Fact]
    public void F4_recently_is_claimed_for_views_a_month_old()
    {
        var r = Make.Recipe();
        var oldViews = Make.History(r, views: 3, viewedDaysAgo: 30);

        var reasons = RecommendationExplainer.Explain(r, oldViews, Lunch, Make.Prefs());

        Assert.Contains("You viewed similar recipes recently", reasons);
    }

    [Fact]
    public void F4_usually_cook_this_for_is_shown_but_never_used_in_the_score()
    {
        var r = Make.Recipe();
        var asLunch = Make.History(r, cooks: 1, cookedDaysAgo: 1);
        asLunch.LastUsedMealType = "lunch";
        var asBreakfast = Make.History(r, cooks: 1, cookedDaysAgo: 1);
        asBreakfast.LastUsedMealType = "breakfast";

        Assert.Contains("You usually cook this for lunch", RecommendationExplainer.Explain(r, asLunch, Lunch, Make.Prefs()));
        Assert.Equal(
            RecipeScorer.Score(r, Make.Features(), asLunch, Lunch, Make.Prefs()),
            RecipeScorer.Score(r, Make.Features(), asBreakfast, Lunch, Make.Prefs()), 6);
    }

    // F5 (low, design): "lose weight" compares one recipe's calories with the DAILY target,
    // so nearly every recipe gets the 1.25 boost. Goals are soft weights, never hard limits.
    [Fact]
    public void F5_daily_target_gives_the_same_boost_to_a_light_and_a_heavy_meal()
    {
        var light = Make.Recipe(kcal: 250);
        var heavy = Make.Recipe(kcal: 1700);
        var prefs = Make.Prefs(goal: "lose weight", calorieTarget: 1800);

        // With equal normalised features, the two recipes score the same.
        Assert.Equal(Score(light, prefs), Score(heavy, prefs), 6);
    }

    // F6 (low): ranking an empty list throws (Min/Max of an empty sequence).
    // HomeViewModel catches the exception, so the user just sees an empty list.
    [Fact]
    public async Task F6_empty_candidate_list_throws_but_the_home_page_catches_it()
    {
        Assert.Throws<InvalidOperationException>(() =>
            RecommendationService.Rank(new List<Recipe>(), Make.Prefs(), new List<UserRecipeInteraction>(), Lunch));

        var home = HomeWith(new[] { Make.Recipe("Tomato Soup") }, Make.Prefs());
        await home.InitializeAsync();
        home.ApplySearch("no recipe has this text");

        Assert.Empty(home.FilteredRecipes);
    }

    // F7 (info, design): history decays below the "never seen" level. One view 15 days ago
    // ranks a recipe lower than a recipe the user never opened.
    [Fact]
    public void F7_an_old_view_ranks_below_a_never_seen_recipe()
    {
        var seen = Make.Recipe("Seen");
        var unseen = Make.Recipe("Unseen");

        var seenScore = RecipeScorer.Score(seen, Make.Features(), Make.History(seen, views: 1, viewedDaysAgo: 15), Lunch, Make.Prefs());
        var unseenScore = RecipeScorer.Score(unseen, Make.Features(), Make.NoHistory(unseen), Lunch, Make.Prefs());

        Assert.True(seenScore < unseenScore);
    }

    // F8 (medium): the Search page ranks by text match only and never applies allergies or
    // dietary rules (SearchViewModel has no access to the user's preferences).
    [Fact]
    public void F8_search_page_shows_recipes_that_break_the_users_allergies()
    {
        var recipes = new ObservableCollection<Recipe> { Make.Recipe("Peanut Noodles", ingredients: "3 tbsp peanut butter") };
        var search = new SearchViewModel(new FakeRecipeService(recipes));

        search.ApplySearch("peanut");

        Assert.Single(search.Results); // shown, even for a user whose profile says "nuts"
    }

    // ---------- small fakes for the view-model tests ----------

    private static HomeViewModel HomeWith(IEnumerable<Recipe> recipes, UserPreferences prefs)
    {
        var store = new FakePreferencesStore(prefs);
        var recommender = new RecommendationService(new NoHistory(), store);
        return new HomeViewModel(new FakeRecipeService(new ObservableCollection<Recipe>(recipes)), recommender, store);
    }

    private sealed class FakeRecipeService(ObservableCollection<Recipe> all) : IRecipeService
    {
        public ObservableCollection<Recipe> GetAllRecipes() => all;
        public ObservableCollection<Recipe> GetSavedRecipes() => new();
        public void SaveRecipe(Recipe recipe) { }
        public void UnsaveRecipe(Recipe recipe) { }
        public bool IsRecipeSaved(Recipe recipe) => false;
    }

    private sealed class FakePreferencesStore(UserPreferences prefs) : IUserPreferencesStore
    {
        public Task<UserPreferences> LoadAsync() => Task.FromResult(prefs);
        public Task SaveAsync(UserPreferences preferences) => Task.CompletedTask;
    }

    private sealed class NoHistory : IUserInteractionService
    {
        public Task RecordViewAsync(string recipeId) => Task.CompletedTask;
        public Task RecordSaveAsync(string recipeId) => Task.CompletedTask;
        public Task RecordCookAsync(string recipeId, string mealType) => Task.CompletedTask;
        public Task<UserRecipeInteraction?> GetAsync(string recipeId) => Task.FromResult<UserRecipeInteraction?>(null);
        public Task<IReadOnlyList<UserRecipeInteraction>> GetAllAsync() =>
            Task.FromResult<IReadOnlyList<UserRecipeInteraction>>(new List<UserRecipeInteraction>());
    }
}
