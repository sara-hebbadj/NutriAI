using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Diagnostics;
using NutriAI.DTOs;
using NutriAI.Models;

namespace NutriAI.Services;

public class ApiRecipeService : IRecipeService
{
    // ✅ SINGLE HttpClient (important for Android)
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private readonly ObservableCollection<Recipe> _allRecipes = new();
    private readonly ObservableCollection<Recipe> _savedRecipes = new();

    private const string ApiKey = "4e7f8a1091d049969c87dfb2e801cc95";
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
    private static string NormalizeDiet(IEnumerable<string>? diets)
    {
        if (diets == null) return "balanced";

        foreach (var d in diets)
        {
            var key = d.ToLowerInvariant().Trim();
            if (DietMap.TryGetValue(key, out var normalized))
                return normalized;
        }

        return "balanced"; // safe default
    }
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




    // =========================
    // LOAD RECIPES (HOME / SEARCH)
    // =========================
    public async Task LoadRecipesAsync(string query = "healthy")
    {
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
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"[API ERROR] LoadRecipesAsync: {ex.Message}");
            _allRecipes.Clear(); // fail gracefully
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
        try
        {
            var url =
                $"https://api.spoonacular.com/recipes/{recipeId}/information" +
                $"?includeNutrition=true&apiKey={ApiKey}";

            var dto =
                await _httpClient.GetFromJsonAsync<ApiRecipeDetailsDto>(url);

            if (dto == null) return null;

            return new Recipe
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
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"[API ERROR] GetRecipeDetailsAsync: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UNEXPECTED ERROR] {ex.Message}");
            return null;
        }
    }

    // =========================
    // HELPER METHODS
    // =========================
    private static int GetNutrient(Nutrition nutrition, string name)
    {
        return (int)(nutrition?.nutrients?
            .FirstOrDefault(n => n.name == name)?.amount ?? 0);
    }
}
