using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using NutriAI.Models;
using NutriAI.Services.Storage;

namespace NutriAI.ViewModels;

public class AllergiesViewModel : INotifyPropertyChanged
{
    private readonly IUserPreferencesStore _store;
    private UserPreferences _preferences = new();

    public AllergiesViewModel(IUserPreferencesStore store)
    {
        _store = store;
        LoadAsync();
    }

    // =========================
    // CHECKBOX BINDINGS
    // =========================

    public bool HasNuts
    {
        get => _preferences.Allergies.Contains("nuts");
        set => SetAllergy("nuts", value);
    }

    public bool HasDairy
    {
        get => _preferences.Allergies.Contains("dairy");
        set => SetAllergy("dairy", value);
    }

    public bool HasEggs
    {
        get => _preferences.Allergies.Contains("eggs");
        set => SetAllergy("eggs", value);
    }

    public bool HasShellfish
    {
        get => _preferences.Allergies.Contains("shellfish");
        set => SetAllergy("shellfish", value);
    }

    public bool HasGluten
    {
        get => _preferences.Allergies.Contains("gluten");
        set => SetAllergy("gluten", value);
    }

    // =========================
    // LOAD / SAVE
    // =========================

    private async void LoadAsync()
    {
        _preferences = await _store.LoadAsync() ?? new UserPreferences();
        OnAllPropertiesChanged();
    }

    private async void SetAllergy(string key, bool enabled)
    {
        if (enabled && !_preferences.Allergies.Contains(key))
            _preferences.Allergies.Add(key);

        if (!enabled && _preferences.Allergies.Contains(key))
            _preferences.Allergies.Remove(key);

        await _store.SaveAsync(_preferences);
        OnAllPropertiesChanged();
    }

    // =========================
    // INotifyPropertyChanged
    // =========================

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnAllPropertiesChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }
}
