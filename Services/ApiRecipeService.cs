using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Diagnostics;
using NutriAI.DTOs;
using NutriAI.Models;
using NutriAI.Services.Caching;

namespace NutriAI.Services;

public class ApiRecipeService : IRecipeService
{
    // ✅ SINGLE HttpClient (important for Android)
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private readonly ICacheService _cache;

    private readonly ObservableCollection<Recipe> _allRecipes = new();
    private readonly ObservableCollection<Recipe> _savedRecipes = new();

    private const string ApiKey = "916a0166fc6f47779e7059fb749bf3bb";

    // =========================
    // CONSTRUCTOR (DI)
    // =========================
    public ApiRecipeService(ICacheService cache)
    {
        _cache = cache;
    }

    // =========================
    // NORMALISATION MAPS
    // =========================
    private static readonly Dictionary<string, string> MealTypeMap = new()
    {
        ["breakfast"] = "breakfast",
        ["brunch"] = "breakfast",
        ["lunch"] = "lunch",
        ["dinner"] = "dinner",
        ["main course"] = "lunch",
        ["side dish"] = "lunch",
        ["soup"] = "lunch",
        ["snack"] = "snack",
        ["dessert"] = "snack"
    };

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

    private static readonly Dictionary<string, string> CuisineMap = new()
    {
        ["italian"] = "italian",
        ["mediterranean"] = "italian",

        ["chinese"] = "asian",
        ["thai"] = "asian",
        ["japanese"] = "asian",
        ["korean"] = "asian",
        ["asian"] = "asian",

        ["middle eastern"] = "middle eastern",
        ["lebanese"] = "middle eastern",
        ["turkish"] = "middle eastern",

        ["american"] = "american",
        ["mexican"] = "american"
    };

    // =========================
    // LOAD RECIPES (HOME / SEARCH)
    // =========================
    public async Task LoadRecipesAsync(string query = "healthy")
    {
        var cacheKey = $"recipes:list:query={query}";

        // 1️ Try cache first
        var (found, cached) =
            await _cache.GetAsync<List<Recipe>>(cacheKey);

        if (found && cached != null)
        {
            _allRecipes.Clear();
            foreach (var r in cached)
                _allRecipes.Add(r);

            Debug.WriteLine("[CACHE] Loaded recipes from cache");
            return;
        }

        // 2️ Cache miss → API
        try
        {
            var url =
                $"https://api.spoonacular.com/recipes/complexSearch" +
                $"?query={query}" +
                $"&number=20" +
                $"&addRecipeInformation=true" +
                $"&addRecipeNutrition=true" +
                $"&apiKey={ApiKey}";

            var response =
                await _httpClient.GetFromJsonAsync<ApiSearchResponse>(url);

            _allRecipes.Clear();

            if (response?.results == null)
                return;

            foreach (var r in response.results)
            {
                _allRecipes.Add(new Recipe
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

            // 3️ Save to cache
            await _cache.SetAsync(
                cacheKey,
                _allRecipes.ToList(),
                TimeSpan.FromHours(12));

            Debug.WriteLine("[CACHE] Recipes saved to cache");
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"[API ERROR] LoadRecipesAsync: {ex.Message}");

            // 4️ Fallback to cache
            var fallback =
                await _cache.GetAsync<List<Recipe>>(cacheKey);

            if (fallback.found && fallback.data != null)
            {
                _allRecipes.Clear();
                foreach (var r in fallback.data)
                    _allRecipes.Add(r);

                Debug.WriteLine("[CACHE] Fallback to cached recipes");
            }
        }
    }

    // =========================
    // INTERFACE IMPLEMENTATION
    // =========================
    public ObservableCollection<Recipe> GetAllRecipes() => _allRecipes;

    public ObservableCollection<Recipe> GetSavedRecipes() => _savedRecipes;

    public void SaveRecipe(Recipe recipe)
    {
        if (!_savedRecipes.Any(r => r.Id == recipe.Id))
            _savedRecipes.Add(recipe);
    }

    public void UnsaveRecipe(Recipe recipe)
    {
        var existing = _savedRecipes.FirstOrDefault(r => r.Id == recipe.Id);
        if (existing != null)
            _savedRecipes.Remove(existing);
    }

    public bool IsRecipeSaved(Recipe recipe) =>
        _savedRecipes.Any(r => r.Id == recipe.Id);

    // =========================
    // DETAILS PAGE
    // =========================
    public async Task<Recipe?> GetRecipeDetailsAsync(string recipeId)
    {
        var cacheKey = $"recipes:details:id={recipeId}";

        // Try cache first
        var (found, cached) =
            await _cache.GetAsync<Recipe>(cacheKey);

        if (found && cached != null)
        {
            Debug.WriteLine("[CACHE] Loaded recipe details from cache");
            return cached;
        }

        // Cache miss → API
        try
        {
            var url =
                $"https://api.spoonacular.com/recipes/{recipeId}/information" +
                $"?includeNutrition=true&apiKey={ApiKey}";

            var dto =
                await _httpClient.GetFromJsonAsync<ApiRecipeDetailsDto>(url);

            if (dto == null) return null;

            var recipe = new Recipe
            {
                Id = dto.id.ToString(),
                Title = dto.title,
                ImageUrl = dto.image,
                CookingTimeMinutes = dto.readyInMinutes,

                Ingredients = dto.extendedIngredients?
                    .Select(i => i.original)
                    .ToList() ?? new(),

                Instructions = dto.analyzedInstructions?
                    .FirstOrDefault()?.steps?
                    .Select(s => s.step)
                    .ToList() ?? new(),

                Calories = GetNutrient(dto.nutrition, "Calories"),
                ProteinGrams = GetNutrient(dto.nutrition, "Protein"),
                CarbsGrams = GetNutrient(dto.nutrition, "Carbohydrates"),
                FatGrams = GetNutrient(dto.nutrition, "Fat")
            };

            //  Save to cache
            await _cache.SetAsync(
                cacheKey,
                recipe,
                TimeSpan.FromDays(7));

            Debug.WriteLine("[CACHE] Recipe details saved");
            return recipe;
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"[API ERROR] GetRecipeDetailsAsync: {ex.Message}");

            // 4️⃣ Fallback
            var fallback =
                await _cache.GetAsync<Recipe>(cacheKey);

            if (fallback.found)
            {
                Debug.WriteLine("[CACHE] Fallback recipe details");
                return fallback.data;
            }

            return null;
        }
    }

    // =========================
    // HELPER METHODS
    // =========================
    private static string NormalizeCategory(
        IEnumerable<string>? apiValues,
        Dictionary<string, string> map)
    {
        var raw = apiValues?
            .FirstOrDefault()?
            .ToLowerInvariant()
            .Trim();

        if (string.IsNullOrEmpty(raw))
            return "";

        return map.TryGetValue(raw, out var normalized)
            ? normalized
            : raw;
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

        return "balanced";
    }

    private static string NormalizeCuisine(IEnumerable<string>? cuisines)
    {
        if (cuisines == null || !cuisines.Any())
            return "other";

        var raw = cuisines
            .FirstOrDefault()?
            .ToLowerInvariant()
            .Trim();

        if (string.IsNullOrEmpty(raw))
            return "other";

        return CuisineMap.TryGetValue(raw, out var normalized)
            ? normalized
            : "other";
    }

    private static int GetNutrient(Nutrition nutrition, string name)
    {
        return (int)(nutrition?.nutrients?
            .FirstOrDefault(n => n.name == name)?.amount ?? 0);
    }
}
