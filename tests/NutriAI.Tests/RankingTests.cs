using NutriAI.Models;
using NutriAI.Preprocessing;
using NutriAI.Services;
using NutriAI.Services.Interactions;
using NutriAI.Services.Recommendation;
using NutriAI.Services.Storage;
using NutriAI.Tests.Helpers;
using NutriAI.Tests.TestData;

namespace NutriAI.Tests;

// Tests for the ranking pipeline in Services/Recommendation/RecommendationService.cs
// and for the test helper that splits the score into factors.
public class RankingTests
{
    [Fact]
    public void Ranking_drops_excluded_recipes_and_sorts_by_score()
    {
        var safe = Make.Recipe("Tomato Rice", ingredients: "1 cup rice");
        var withNuts = Make.Recipe("Almond Cake", ingredients: "100 g ground almonds");
        var prefs = Make.Prefs(allergies: new[] { "nuts" });

        var ranked = RecommendationService.Rank(new[] { safe, withNuts }, prefs, new List<UserRecipeInteraction>(), MealContext.Lunch);

        Assert.Equal(new[] { safe.Id }, ranked.Select(r => r.Id));
    }

    [Fact]
    public void Cooked_beats_saved_beats_viewed_beats_unseen()
    {
        var unseen = Make.Recipe("A");
        var viewed = Make.Recipe("B");
        var saved = Make.Recipe("C");
        var cooked = Make.Recipe("D");
        var history = new List<UserRecipeInteraction>
        {
            Make.History(viewed, views: 1, viewedDaysAgo: 1),
            Make.History(saved, views: 1, saves: 1, viewedDaysAgo: 1),
            Make.History(cooked, views: 1, cooks: 1, viewedDaysAgo: 1, cookedDaysAgo: 1),
        };

        var ranked = RecommendationService.Rank(new[] { unseen, viewed, saved, cooked }, Make.Prefs(), history, MealContext.Lunch);

        Assert.Equal(new[] { "D", "C", "B", "A" }, ranked.Select(r => r.Title));
    }

    [Fact]
    public void Every_ranked_recipe_gets_at_least_one_explanation()
    {
        var recipes = SyntheticRecipes.FreshCopy().Select(x => x.Recipe).ToList();

        var ranked = RecommendationService.Rank(recipes, Make.Prefs(), new List<UserRecipeInteraction>(), MealContext.Dinner);

        Assert.NotEmpty(ranked);
        Assert.All(ranked, r => Assert.InRange(r.RecommendationReasons.Count, 1, 3));
    }

    [Fact]
    public void A_single_candidate_does_not_break_normalisation()
    {
        // With one recipe, min == max for every feature; the normaliser returns 0 instead of dividing by zero.
        var only = Make.Recipe("Only Dish");

        var ranked = RecommendationService.Rank(new[] { only }, Make.Prefs(goal: "gain muscle"), new List<UserRecipeInteraction>(), MealContext.Dinner);

        Assert.Single(ranked);
    }

    [Fact]
    public async Task RankAsync_uses_the_stored_preferences_and_history()
    {
        var milkDish = Make.Recipe("Rice Pudding", ingredients: "1 litre milk");
        var plain = Make.Recipe("Plain Rice");
        var store = new FakePreferencesStore(Make.Prefs(allergies: new[] { "dairy" }));
        var service = new RecommendationService(new FakeInteractions(), store);

        // RankAsync ignores the preferences argument and always reloads them from the store.
        var ranked = await service.RankAsync(new[] { milkDish, plain }, new UserPreferences());

        Assert.Equal(new[] { plain.Id }, ranked.Select(r => r.Id));
    }

    [Fact]
    public void ScoreFactors_helper_matches_the_real_scorer()
    {
        // Guards the test helper: if RecipeScorer's formula changes, this fails first.
        var random = new Random(7);
        var catalogue = SyntheticRecipes.FreshCopy();
        var recipes = catalogue.Select(x => x.Recipe).ToList();
        var stats = StatsFor(recipes);
        var profiles = SyntheticProfiles.Build(catalogue);

        for (var n = 0; n < 3000; n++)
        {
            var recipe = recipes[random.Next(recipes.Count)];
            var prefs = profiles[random.Next(profiles.Count)].Preferences;
            var context = (MealContext)random.Next(4);
            var history = Make.History(recipe, random.Next(0, 5), random.Next(0, 2), random.Next(0, 3),
                viewedDaysAgo: random.Next(0, 40), cookedDaysAgo: random.NextDouble() < 0.5 ? null : random.Next(0, 40));
            var features = RecipeFeatureNormalizer.FromRecipe(recipe, stats);

            var real = RecipeScorer.Score(recipe, features, history, context, prefs);
            var mirrored = ScoreFactors.Compute(recipe, features, history, context, prefs).Total;

            // Tiny differences come from DateTime.UtcNow moving on between the two calls.
            Assert.Equal(real, mirrored, tolerance: 1e-6 * Math.Max(1.0, real));
        }
    }

    [Fact]
    public void ApiRecipeService_compiles_without_a_hard_coded_key()
    {
        // The service is compiled into this test project, so this file proves the
        // environment-variable change builds. The key itself is checked in ConfigurationTests.
        Assert.Equal("SPOONACULAR_API_KEY", SpoonacularSettings.ApiKeyVariable);
        Assert.NotNull(typeof(ApiRecipeService).GetMethod("LoadRecipesAsync"));
    }

    internal static NutritionStats StatsFor(IReadOnlyList<Recipe> recipes) => new()
    {
        MinCalories = recipes.Min(r => r.Calories), MaxCalories = recipes.Max(r => r.Calories),
        MinProtein = recipes.Min(r => r.ProteinGrams), MaxProtein = recipes.Max(r => r.ProteinGrams),
        MinCarbs = recipes.Min(r => r.CarbsGrams), MaxCarbs = recipes.Max(r => r.CarbsGrams),
        MinFat = recipes.Min(r => r.FatGrams), MaxFat = recipes.Max(r => r.FatGrams),
        MinTime = recipes.Min(r => r.CookingTimeMinutes), MaxTime = recipes.Max(r => r.CookingTimeMinutes),
    };

    private sealed class FakePreferencesStore(UserPreferences prefs) : IUserPreferencesStore
    {
        public Task<UserPreferences> LoadAsync() => Task.FromResult(prefs);
        public Task SaveAsync(UserPreferences preferences) => Task.CompletedTask;
    }

    private sealed class FakeInteractions : IUserInteractionService
    {
        public Task RecordViewAsync(string recipeId) => Task.CompletedTask;
        public Task RecordSaveAsync(string recipeId) => Task.CompletedTask;
        public Task RecordCookAsync(string recipeId, string mealType) => Task.CompletedTask;
        public Task<UserRecipeInteraction?> GetAsync(string recipeId) => Task.FromResult<UserRecipeInteraction?>(null);
        public Task<IReadOnlyList<UserRecipeInteraction>> GetAllAsync() =>
            Task.FromResult<IReadOnlyList<UserRecipeInteraction>>(new List<UserRecipeInteraction>());
    }
}
