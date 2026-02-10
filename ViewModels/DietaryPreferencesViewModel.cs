using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using NutriAI.Models;
using NutriAI.Services.Storage;

namespace NutriAI.ViewModels;

public class DietaryPreferencesViewModel : INotifyPropertyChanged
{
    private readonly IUserPreferencesStore _store;
    private UserPreferences _preferences = new();

    public DietaryPreferencesViewModel(IUserPreferencesStore store)
    {
        _store = store;
        LoadAsync();
    }

    // =========================
    // CHECKBOX BINDINGS
    // =========================
    public bool IsVegetarian
    {
        get => _preferences.DietaryPreferences.Contains("vegetarian");
        set => SetPreference("vegetarian", value);
    }

    public bool IsVegan
    {
        get => _preferences.DietaryPreferences.Contains("vegan");
        set => SetPreference("vegan", value);
    }

    public bool IsHalal
    {
        get => _preferences.DietaryPreferences.Contains("halal");
        set => SetPreference("halal", value);
    }

    public bool IsGlutenFree
    {
        get => _preferences.DietaryPreferences.Contains("gluten-free");
        set => SetPreference("gluten-free", value);
    }

    public bool IsDairyFree
    {
        get => _preferences.DietaryPreferences.Contains("dairy-free");
        set => SetPreference("dairy-free", value);
    }

    public bool IsKeto
    {
        get => _preferences.DietaryPreferences.Contains("keto");
        set => SetPreference("keto", value);
    }

    // =========================
    // LOAD / SAVE
    // =========================
    private async void LoadAsync()
    {
        _preferences = await _store.LoadAsync() ?? new UserPreferences();
        OnAllPropertiesChanged();
    }

    private async void SetPreference(string key, bool enabled)
    {
        if (enabled && !_preferences.DietaryPreferences.Contains(key))
            _preferences.DietaryPreferences.Add(key);

        if (!enabled && _preferences.DietaryPreferences.Contains(key))
            _preferences.DietaryPreferences.Remove(key);

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
