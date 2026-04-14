// Authorship note:
// Microsoft Learn documentation was used in this file for local file storage,
// JSON serialization, and async file access in .NET.
// Copilot was used to help draft and refine the file-based preferences store.
// The decision to persist user dietary preferences and goals locally,
// and to use them as transparent inputs to recommendation filtering and ranking,
// was made and integrated by the author.

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
    // Store user preferences in a JSON file inside the app's local data folder.
    private readonly string _path =
        Path.Combine(FileSystem.AppDataDirectory, "user_preferences.json");

    public async Task<UserPreferences> LoadAsync()
    {
        // If no preferences file exists yet, return a default preferences object.
        if (!File.Exists(_path))
            return new UserPreferences();

        // Read the saved JSON file and convert it back into a UserPreferences object.
        var json = await File.ReadAllTextAsync(_path);
        return JsonSerializer.Deserialize<UserPreferences>(json)
               ?? new UserPreferences();
    }

    public async Task SaveAsync(UserPreferences preferences)
    {
        // Convert the preferences object into JSON and save it locally
        // so the user's current settings can be reused later.
        var json = JsonSerializer.Serialize(preferences);
        await File.WriteAllTextAsync(_path, json);
    }
}
