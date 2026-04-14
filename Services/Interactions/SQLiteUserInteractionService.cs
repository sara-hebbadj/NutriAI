// Authorship note:
// Microsoft Learn documentation and SQLite / sqlite-net examples were used in this file
// for SQLiteAsyncConnection, table creation, async inserts, and local database storage in .NET MAUI.
// Copilot was used to help draft and refine the SQLite service implementation.
// The NutriAI-specific interaction model - tracking views, saves, cooks, timestamps,
// and last used meal type as implicit feedback signals - was designed and adapted by the author.

using SQLite;
using NutriAI.Models;
using System.Diagnostics;

namespace NutriAI.Services.Interactions;

public class SQLiteUserInteractionService : IUserInteractionService
{
    private readonly SQLiteAsyncConnection _db;

    public SQLiteUserInteractionService()
    {
        // Store the local SQLite database inside the app data folder.
        var path = Path.Combine(
            FileSystem.AppDataDirectory,
            "nutriai_v2.db");

        // Open an async SQLite connection to the local database file.
        _db = new SQLiteAsyncConnection(path);
        // Create the interaction table if it does not already exist.
        _db.CreateTableAsync<UserRecipeInteraction>().Wait();
    }

    // =========================
    // VIEW
    // =========================
    public async Task RecordViewAsync(string recipeId)
    {
        // Get the existing row for this recipe, or create a new one if none exists yet.
        var row = await GetOrCreateAsync(recipeId);

        // A view is treated as a weaker signal of interest.
        row.ViewCount++;
        // Store when the recipe was last viewed so recency can be used later.
        row.LastViewedAt = DateTime.UtcNow;

        await _db.InsertOrReplaceAsync(row);
    }

    // =========================
    // SAVE
    // =========================
    public async Task RecordSaveAsync(string recipeId)
    {
        // Get the existing row for this recipe, or create a new one if needed.
        var row = await GetOrCreateAsync(recipeId);

        // A save is treated as a stronger signal than a simple view.
        row.SaveCount++;

        await _db.InsertOrReplaceAsync(row);
    }

    // =========================
    // COOK (WITH CONTEXT)
    // =========================
    public async Task RecordCookAsync(string recipeId, string mealType)
    {
        // Get the existing row for this recipe, or create a new one if needed.
        var row = await GetOrCreateAsync(recipeId);

        // Cooking is treated as the strongest behavioural signal,
        row.CookCount++;
        // Store when the recipe was last cooked.
        row.LastCookedAt = DateTime.UtcNow;
        // Save the meal context in a normalized form for later use in recommendation.
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
        // Try to find an existing interaction row for this recipe.
        var row = await GetAsync(recipeId);
        // If none exists yet, start a new record with the recipe id.
        return row ?? new UserRecipeInteraction { RecipeId = recipeId };
    }

}
