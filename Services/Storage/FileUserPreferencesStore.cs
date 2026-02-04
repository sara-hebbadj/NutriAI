using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using NutriAI.Models;

namespace NutriAI.Services.Storage;

public class FileUserPreferencesStore : IUserPreferencesStore
{
    private readonly string _path =
        Path.Combine(FileSystem.AppDataDirectory, "user_preferences.json");

    public async Task<UserPreferences> LoadAsync()
    {
        if (!File.Exists(_path))
            return new UserPreferences();

        var json = await File.ReadAllTextAsync(_path);
        return JsonSerializer.Deserialize<UserPreferences>(json)
               ?? new UserPreferences();
    }

    public async Task SaveAsync(UserPreferences preferences)
    {
        var json = JsonSerializer.Serialize(preferences);
        await File.WriteAllTextAsync(_path, json);
    }
}
