// Authorship note:
// Microsoft Learn documentation was used in this file for FileSystem.AppDataDirectory,
// System.Text.Json, File.Exists, File.ReadAllTextAsync, and File.WriteAllTextAsync.
// Copilot was used to help draft the local JSON storage pattern.
// The decision to store saved recipes locally in NutriAI rather than in cloud storage
// was made by the author and fits the single-user prototype design.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using NutriAI.Models;

namespace NutriAI.Services.Storage;

public class FileSavedRecipeStore : ISavedRecipeStore
{
    // Store saved recipes in a JSON file inside the app's local data folder.
    private readonly string _path =
        Path.Combine(FileSystem.AppDataDirectory, "saved_recipes.json");

    public async Task<IReadOnlyList<Recipe>> LoadAsync()
    {
        // If the file does not exist yet, return an empty list
        // because the user has not saved any recipes so far.
        if (!File.Exists(_path))
            return new List<Recipe>();

        // Read the saved JSON file and convert it back into a list of recipes.
        var json = await File.ReadAllTextAsync(_path);
        return JsonSerializer.Deserialize<List<Recipe>>(json) ?? new List<Recipe>();
    }

    public async Task SaveAsync(IEnumerable<Recipe> recipes)
    {
        // Convert the recipes into JSON and overwrite the local file
        // so the latest saved recipe state is kept on the device.
        var json = JsonSerializer.Serialize(recipes);
        await File.WriteAllTextAsync(_path, json);
    }
}

