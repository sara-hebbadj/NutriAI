using System.Windows.Input;

namespace NutriAI.ViewModels;

public class DietaryPreferencesViewModel : BaseViewModel
{
    private string _preference;
    public string Preference
    {
        get => _preference;
        set => SetProperty(ref _preference, value);
    }

    public ICommand SaveCommand { get; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public DietaryPreferencesViewModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        Preference = "Vegetarian";

        SaveCommand = new Command(async () =>
        {
            await Shell.Current.DisplayAlert("Saved", "Dietary preference updated.", "OK");
        });
    }
}