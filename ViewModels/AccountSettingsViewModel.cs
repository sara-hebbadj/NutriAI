using System.Windows.Input;
using NutriAI.Models;
using NutriAI.Services.Storage;

namespace NutriAI.ViewModels;

public class AccountSettingsViewModel : BaseViewModel
{
    private readonly IUserPreferencesStore _preferencesStore;

    private string _name = "";
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    private int? _weightKg;
    public int? WeightKg
    {
        get => _weightKg;
        set { _weightKg = value; OnPropertyChanged(); }
    }

    private int? _heightCm;
    public int? HeightCm
    {
        get => _heightCm;
        set { _heightCm = value; OnPropertyChanged(); }
    }

    public ICommand SaveCommand { get; }

    public AccountSettingsViewModel(IUserPreferencesStore preferencesStore)
    {
        _preferencesStore = preferencesStore;

        SaveCommand = new Command(async () => await SaveAsync());
    }

    public async Task RefreshAsync()
    {
        var prefs = await _preferencesStore.LoadAsync() ?? new UserPreferences();

        Name = prefs.Name;
        WeightKg = prefs.WeightKg;
        HeightCm = prefs.HeightCm;
    }

    private async Task SaveAsync()
    {
        var prefs = await _preferencesStore.LoadAsync() ?? new UserPreferences();

        prefs.Name = Name;
        prefs.WeightKg = WeightKg;
        prefs.HeightCm = HeightCm;

        await _preferencesStore.SaveAsync(prefs);

        await Shell.Current.DisplayAlert("Saved", "Account settings updated", "OK");
    }
}
