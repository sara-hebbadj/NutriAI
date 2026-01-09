using System.Collections.ObjectModel;
using System.Net.Http.Json;
using NutriAI.DTOs;
using NutriAI.Models;

namespace NutriAI.Services;

public class ApiRecipeService : IRecipeService
{
    private readonly HttpClient _httpClient = new();
    private readonly ObservableCollection<Recipe> _allRecipes = new();
    private readonly ObservableCollection<Recipe> _savedRecipes = new();

    private const string ApiKey = "fec5d11c6dfc4a518efb3fced22fc298";

    // =========================
    // LOAD RECIPES (HOME / SEARCH)
    // =========================
    public async Task LoadRecipesAsync(string query = "healthy")
    {
        var url =
            $"https://api.spoonacular.com/recipes/complexSearch" +
            $"?query={query}" +
            $"&number=10" +
            $"&addRecipeInformation=true" +
            $"&addRecipeNutrition=true" +
            $"&apiKey={ApiKey}";

        var response =
            await _httpClient.GetFromJsonAsync<ApiSearchResponse>(url);

        _allRecipes.Clear();

        foreach (var dto in response.results)
        {
            _allRecipes.Add(MapSearchDtoToRecipe(dto));
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
    public async Task<Recipe> GetRecipeDetailsAsync(string recipeId)
    {
        var url =
            $"https://api.spoonacular.com/recipes/{recipeId}/information" +
            $"?includeNutrition=true&apiKey={ApiKey}";

        var dto =
            await _httpClient.GetFromJsonAsync<ApiRecipeDetailsDto>(url);

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

    // =========================
    // MAPPING HELPERS
    // =========================
    private static Recipe MapSearchDtoToRecipe(ApiSearchRecipeDto dto)
    {
        return new Recipe
        {
            Id = dto.id.ToString(),
            Title = dto.title,
            ImageUrl = dto.image,
            CookingTimeMinutes = dto.readyInMinutes,
            Calories = GetNutrient(dto.nutrition, "Calories"),
            ProteinGrams = GetNutrient(dto.nutrition, "Protein"),
            CarbsGrams = GetNutrient(dto.nutrition, "Carbohydrates"),
            FatGrams = GetNutrient(dto.nutrition, "Fat")
        };
    }

    private static int GetNutrient(Nutrition nutrition, string name)
    {
        return (int)(nutrition?.nutrients?
            .FirstOrDefault(n => n.name == name)?.amount ?? 0);
    }
}
