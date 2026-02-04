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
    private readonly string _path =
        Path.Combine(FileSystem.AppDataDirectory, "saved_recipes.json");

    public async Task<IReadOnlyList<Recipe>> LoadAsync()
    {
        if (!File.Exists(_path))
            return new List<Recipe>();

        var json = await File.ReadAllTextAsync(_path);
        return JsonSerializer.Deserialize<List<Recipe>>(json) ?? new List<Recipe>();
    }

    public async Task SaveAsync(IEnumerable<Recipe> recipes)
    {
        var json = JsonSerializer.Serialize(recipes);
        await File.WriteAllTextAsync(_path, json);
    }
}

