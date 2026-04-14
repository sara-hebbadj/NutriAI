// Authorship note:
// Microsoft Learn documentation was used in this file for HttpClient,
// GetFromJsonAsync, ObservableCollection, LINQ, async/await, and TimeSpan usage.
// Copilot was used to help draft and refine parts of the API service structure,
// including fetch methods, fallback flow, DTO-to-model mapping, and cache usage patterns.
// The NutriAI-specific decisions in this file - using Spoonacular as the recipe source,
// choosing which recipe fields to map, defining fallback queries, normalizing meal/diet/cuisine
// categories, and integrating caching and saved-recipe persistence into the app -
// were selected, adapted, and tested by the author.

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Diagnostics;
using NutriAI.DTOs;
using NutriAI.Models;
using NutriAI.Services.Caching;
using NutriAI.Services.Storage;

namespace NutriAI.Services;

public class ApiRecipeService : IRecipeService
{
    // Reuse one HttpClient instance for API calls.
    // A timeout is included so the app does not wait too long on failed requests.
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private readonly ICacheService _cache;
    private readonly ISavedRecipeStore _savedRecipeStore;

    // Holds the recipes shown on the main browse/home views.
    private readonly ObservableCollection<Recipe> _allRecipes = new();

    // Holds the user's locally saved recipes.
    private ObservableCollection<Recipe> _savedRecipes = new();

    // Spoonacular API key used for recipe requests.
    private const string ApiKey = "4e7f8a1091d049969c87dfb2e801cc95";

    // =========================
    // CONSTRUCTOR
    // =========================
    public ApiRecipeService(
        ICacheService cache,
        ISavedRecipeStore savedRecipeStore)
    {
        _cache = cache;
        _savedRecipeStore = savedRecipeStore;

        // Load previously saved recipes from local storage when the service starts.
        LoadSavedRecipes();
    }

    // =========================
    // LOAD SAVED RECIPES 
    // =========================
    private async void LoadSavedRecipes()
    {
        var saved = await _savedRecipeStore.LoadAsync();
        _savedRecipes = new ObservableCollection<Recipe>(saved);
    }

    // =========================
    // INTERFACE IMPLEMENTATION
    // =========================

    // Return the full in-memory recipe collection used by the UI.
    public ObservableCollection<Recipe> GetAllRecipes() => _allRecipes;

    // Return the in-memory saved recipes collection used by the UI.
    public ObservableCollection<Recipe> GetSavedRecipes() => _savedRecipes;

    public async void SaveRecipe(Recipe recipe)
    {
        // Prevent duplicate saves for the same recipe.
        if (_savedRecipes.Any(r => r.Id == recipe.Id))
            return;

        _savedRecipes.Add(recipe);

        // Persist the updated saved-recipes list locally.
        await _savedRecipeStore.SaveAsync(_savedRecipes);
    }

    public async void UnsaveRecipe(Recipe recipe)
    {
        var existing = _savedRecipes.FirstOrDefault(r => r.Id == recipe.Id);
        if (existing == null)
            return;

        _savedRecipes.Remove(existing);

        // Persist the updated saved-recipes list locally.
        await _savedRecipeStore.SaveAsync(_savedRecipes);
    }

    public bool IsRecipeSaved(Recipe recipe)
        => _savedRecipes.Any(r => r.Id == recipe.Id);

    // =========================
    // LOAD RECIPES (HOME / SEARCH)
    // =========================
    public async Task LoadRecipesAsync(string query = "healthy")
    {
        var cacheKey = $"recipes:list:query={query}";

        // Try local cache first so repeated searches do not keep hitting the API.
        var (found, cached) =
            await _cache.GetAsync<List<Recipe>>(cacheKey);

        if (found && cached != null && cached.Any())
        {
            _allRecipes.Clear();
            foreach (var r in cached)
                _allRecipes.Add(r);
            return;
        }

        try
        {
            var response = await FetchRecipesAsync(query);

            // If the user's original query returns nothing, try a broader fallback query.
            if (response?.results == null || !response.results.Any())
            {
                Debug.WriteLine($"[API] No results for query '{query}', trying fallback query 'healthy'...");
                response = await FetchRecipesAsync("healthy");
            }

            // Second fallback to reduce the chance of showing a blank page.
            if (response?.results == null || !response.results.Any())
            {
                Debug.WriteLine("[API] No results for fallback query 'healthy', trying fallback query 'meal'...");
                response = await FetchRecipesAsync("meal");
            }

            // If no results are found even after fallbacks, keep the existing UI state.
            if (response?.results == null || !response.results.Any())
            {
                Debug.WriteLine("[API] No recipes returned after fallbacks. Keeping existing recipes.");
                return;
            }

            var freshRecipes = new List<Recipe>();

            foreach (var r in response.results)
            {
                // Map the external API DTO into NutriAI's internal Recipe model.
                freshRecipes.Add(new Recipe
                {
                    Id = r.id.ToString(),
                    Title = r.title,
                    ImageUrl = r.image,
                    CookingTimeMinutes = r.readyInMinutes,
                    Calories = GetNutrient(r.nutrition, "Calories"),
                    ProteinGrams = GetNutrient(r.nutrition, "Protein"),
                    CarbsGrams = GetNutrient(r.nutrition, "Carbohydrates"),
                    FatGrams = GetNutrient(r.nutrition, "Fat"),
                    MealType = NormalizeCategory(r.dishTypes, MealTypeMap),
                    Diet = NormalizeDiet(r.diets),
                    Cuisine = NormalizeCuisine(r.cuisines)
                });
            }

            // If the mapping step somehow produces no usable recipes, avoid clearing the page.
            if (!freshRecipes.Any())
            {
                Debug.WriteLine("[API] Mapped recipe list is empty. Keeping existing recipes.");
                return;
            }

            _allRecipes.Clear();
            foreach (var recipe in freshRecipes)
                _allRecipes.Add(recipe);

            // Cache the mapped recipe list locally for faster future loading.
            await _cache.SetAsync(
                cacheKey,
                freshRecipes,
                TimeSpan.FromHours(12));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[API ERROR] {ex.Message}");
        }
    }

    private async Task<ApiSearchResponse?> FetchRecipesAsync(string query)
    {
        var url =
            $"https://api.spoonacular.com/recipes/complexSearch" +
            $"?query={query}&number=40&addRecipeInformation=true" +
            $"&addRecipeNutrition=true&apiKey={ApiKey}";

        // Deserialize the JSON API response directly into the DTO type.
        return await _httpClient.GetFromJsonAsync<ApiSearchResponse>(url);
    }

    // =========================
    // DETAILS PAGE 
    // =========================
    public async Task<Recipe?> GetRecipeDetailsAsync(string recipeId)
    {
        var cacheKey = $"recipes:details:id={recipeId}";

        // Use cached details first if available.
        var (found, cached) =
            await _cache.GetAsync<Recipe>(cacheKey);

        if (found && cached != null)
            return cached;

        try
        {
            var url =
                $"https://api.spoonacular.com/recipes/{recipeId}/information" +
                $"?includeNutrition=true&apiKey={ApiKey}";

            var dto =
                await _httpClient.GetFromJsonAsync<ApiRecipeDetailsDto>(url);

            if (dto == null) return null;

            // Map the details DTO into the internal Recipe model used by the app.
            var recipe = new Recipe
            {
                Id = dto.id.ToString(),
                Title = dto.title,
                ImageUrl = dto.image,
                CookingTimeMinutes = dto.readyInMinutes,
                Ingredients = dto.extendedIngredients?
                    .Select(i => i.original).ToList() ?? new(),
                Instructions = string.Join(
                    "\n\n",
                    dto.analyzedInstructions?
                        .FirstOrDefault()?.steps?
                        .Select(s => s.step)
                        ?? Enumerable.Empty<string>()
                ),

                Calories = GetNutrient(dto.nutrition, "Calories"),
                ProteinGrams = GetNutrient(dto.nutrition, "Protein"),
                CarbsGrams = GetNutrient(dto.nutrition, "Carbohydrates"),
                FatGrams = GetNutrient(dto.nutrition, "Fat")
            };

            // Cache detailed recipe data for longer because it changes less often.
            await _cache.SetAsync(
                cacheKey,
                recipe,
                TimeSpan.FromDays(7));

            return recipe;
        }
        catch
        {
            return null;
        }
    }

    // =========================
    // HELPERS 
    // =========================

    // Map Spoonacular dish-type labels into simpler NutriAI categories.
    private static readonly Dictionary<string, string> MealTypeMap = new()
    {
        ["breakfast"] = "breakfast",
        ["brunch"] = "breakfast",
        ["lunch"] = "lunch",
        ["dinner"] = "dinner",
        ["main course"] = "meal",
        ["side dish"] = "meal",
        ["soup"] = "meal",
        ["snack"] = "snack",
        ["dessert"] = "snack"
    };

    // Map API diet labels into the simpler labels used inside NutriAI.
    private static readonly Dictionary<string, string> DietMap = new()
    {
        ["vegetarian"] = "vegetarian",
        ["vegan"] = "vegan",
        ["ketogenic"] = "keto",
        ["keto"] = "keto",
        ["pescatarian"] = "pescatarian",
        ["gluten free"] = "gluten free",
        ["dairy free"] = "dairy free",
        ["whole30"] = "balanced"
    };

    // Map API cuisine labels into a smaller set of cuisine categories.
    private static readonly Dictionary<string, string> CuisineMap = new()
    {
        ["italian"] = "italian",
        ["mediterranean"] = "italian",
        ["chinese"] = "asian",
        ["thai"] = "asian",
        ["japanese"] = "asian",
        ["korean"] = "asian",
        ["middle eastern"] = "middle eastern",
        ["lebanese"] = "middle eastern",
        ["turkish"] = "middle eastern",
        ["american"] = "american",
        ["mexican"] = "american"
    };

    private static string NormalizeCategory(
        IEnumerable<string>? apiValues,
        Dictionary<string, string> map)
    {
        if (apiValues == null || !apiValues.Any())
            return "";

        foreach (var value in apiValues)
        {
            var raw = value.ToLowerInvariant().Trim();
            if (map.TryGetValue(raw, out var normalized))
                return normalized;
        }

        // If no custom mapping exists, return the first normalized value from the API.
        return apiValues.FirstOrDefault()?.ToLowerInvariant().Trim() ?? "";
    }

    private static string NormalizeDiet(IEnumerable<string>? diets)
    {
        if (diets == null) return "balanced";

        foreach (var d in diets)
        {
            var key = d.ToLowerInvariant().Trim();
            if (DietMap.TryGetValue(key, out var normalized))
                return normalized;
        }

        // Default to "balanced" when no known diet label is matched.
        return "balanced";
    }

    private static string NormalizeCuisine(IEnumerable<string>? cuisines)
    {
        if (cuisines == null || !cuisines.Any())
            return "other";

        var raw = cuisines.First().ToLowerInvariant().Trim();

        return CuisineMap.TryGetValue(raw, out var normalized)
            ? normalized
            : "other";
    }

    private static int GetNutrient(Nutrition nutrition, string name)
    {
        // Extract one named nutrient amount from the API nutrition object.
        return (int)(nutrition?.nutrients?
            .FirstOrDefault(n => n.name == name)?.amount ?? 0);
    }
}