using System.Windows.Input;
using NutriAI.Models;
using NutriAI.Services.Storage;

namespace NutriAI.ViewModels;

public class HealthGoalsViewModel
{
    private readonly IUserPreferencesStore _store;

    public bool IsLoseWeight { get; set; }
    public bool IsMaintain { get; set; }
    public bool IsGainMuscle { get; set; }

    public string? DailyCalories { get; set; }

    public ICommand SaveCommand { get; }

    public HealthGoalsViewModel(IUserPreferencesStore store)
    {
        _store = store;
        SaveCommand = new Command(async () => await SaveAsync());
        LoadAsync();
    }

    private async void LoadAsync()
    {
        var prefs = await _store.LoadAsync();
        if (prefs == null) return;

        IsLoseWeight = prefs.Goal == "lose weight";
        IsMaintain = prefs.Goal == "maintain";
        IsGainMuscle = prefs.Goal == "gain muscle";

        DailyCalories = prefs.DailyCalorieTarget?.ToString();
    }

    private async Task SaveAsync()
    {
        var prefs = await _store.LoadAsync() ?? new UserPreferences();

        prefs.Goal =
            IsLoseWeight ? "lose weight" :
            IsGainMuscle ? "gain muscle" :
            "maintain";

        if (int.TryParse(DailyCalories, out var calories))
            prefs.DailyCalorieTarget = calories;

        await _store.SaveAsync(prefs);
    }
}
