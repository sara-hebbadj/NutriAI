// Portfolio change (October 2026, added with a coding agent):
// The dissertation version kept the Spoonacular API key as a constant in ApiRecipeService.cs.
// The key now comes from an environment variable so it is never stored in the repository.
// See README.md -> "How to run" for the one-line setup on Windows.

using System.Diagnostics;

namespace NutriAI.Services;

public static class SpoonacularSettings
{
    // Name of the environment variable that holds the personal Spoonacular key.
    public const string ApiKeyVariable = "SPOONACULAR_API_KEY";

    // Returns the key, or an empty string when it is not configured.
    // With an empty key Spoonacular answers 401, ApiRecipeService catches the error,
    // and the app keeps showing cached recipes (or an empty list) instead of crashing.
    public static string GetApiKey()
    {
        var key = Environment.GetEnvironmentVariable(ApiKeyVariable);

        if (string.IsNullOrWhiteSpace(key))
        {
            Debug.WriteLine($"[CONFIG] {ApiKeyVariable} is not set. Recipe requests will fail.");
            return string.Empty;
        }

        return key.Trim();
    }
}
