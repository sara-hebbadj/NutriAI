using SQLite;
using NutriAI.Models;

namespace NutriAI.Services.Interactions;

public class SQLiteUserInteractionService : IUserInteractionService
{
    private readonly SQLiteAsyncConnection _db;

    public SQLiteUserInteractionService()
    {
        var path = Path.Combine(
            FileSystem.AppDataDirectory,
            "nutriai_v2.db");

        _db = new SQLiteAsyncConnection(path);
        _db.CreateTableAsync<UserRecipeInteraction>().Wait();
    }

    // =========================
    // VIEW
    // =========================
    public async Task RecordViewAsync(string recipeId)
    {
        var row = await GetOrCreateAsync(recipeId);

        row.ViewCount++;
        row.LastViewedAt = DateTime.UtcNow;

        await _db.InsertOrReplaceAsync(row);
    }

    // =========================
    // SAVE
    // =========================
    public async Task RecordSaveAsync(string recipeId)
    {
        var row = await GetOrCreateAsync(recipeId);

        row.SaveCount++;

        await _db.InsertOrReplaceAsync(row);
    }

    // =========================
    // COOK (WITH CONTEXT)
    // =========================
    public async Task RecordCookAsync(string recipeId, string mealType)
    {
        var row = await GetOrCreateAsync(recipeId);

        row.CookCount++;
        row.LastCookedAt = DateTime.UtcNow;
        row.LastUsedMealType =
            string.IsNullOrWhiteSpace(mealType)
                ? "unknown"
                : mealType.ToLowerInvariant();

        await _db.InsertOrReplaceAsync(row);
    }

    // =========================
    // QUERIES
    // =========================
    public Task<UserRecipeInteraction?> GetAsync(string recipeId)
        => _db.Table<UserRecipeInteraction>()
              .FirstOrDefaultAsync(x => x.RecipeId == recipeId);

    public async Task<IReadOnlyList<UserRecipeInteraction>> GetAllAsync()
        => await _db.Table<UserRecipeInteraction>().ToListAsync();

    // =========================
    // HELPERS
    // =========================
    private async Task<UserRecipeInteraction> GetOrCreateAsync(string recipeId)
    {
        var row = await GetAsync(recipeId);
        return row ?? new UserRecipeInteraction { RecipeId = recipeId };
    }
}
