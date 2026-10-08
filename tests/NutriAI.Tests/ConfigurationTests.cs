using NutriAI.Services;

namespace NutriAI.Tests;

// The Spoonacular key now comes from the SPOONACULAR_API_KEY environment variable
// (Services/SpoonacularSettings.cs) instead of a constant in the source code.
public class ConfigurationTests
{
    [Fact]
    public void Key_is_read_from_the_environment_variable_and_trimmed()
    {
        var before = Environment.GetEnvironmentVariable(SpoonacularSettings.ApiKeyVariable);
        try
        {
            Environment.SetEnvironmentVariable(SpoonacularSettings.ApiKeyVariable, "  test-value-not-a-real-key  ");
            Assert.Equal("test-value-not-a-real-key", SpoonacularSettings.GetApiKey());

            Environment.SetEnvironmentVariable(SpoonacularSettings.ApiKeyVariable, null);
            Assert.Equal(string.Empty, SpoonacularSettings.GetApiKey());
        }
        finally
        {
            Environment.SetEnvironmentVariable(SpoonacularSettings.ApiKeyVariable, before);
        }
    }

    [Fact]
    public void No_32_character_hex_key_is_left_in_the_app_source()
    {
        // Spoonacular keys are 32 hexadecimal characters. Scan every C# file of the app.
        var repoRoot = FindRepoRoot();
        var pattern = new System.Text.RegularExpressions.Regex(@"\b[0-9a-fA-F]{32}\b");
        var offenders = Directory.EnumerateFiles(repoRoot, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                     && !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Where(f => pattern.IsMatch(File.ReadAllText(f)))
            .Select(f => Path.GetRelativePath(repoRoot, f))
            .ToList();

        Assert.Empty(offenders);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "NutriAI.csproj")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("NutriAI.csproj not found above the test folder");
    }
}
