using System.Windows.Input;

namespace NutriAI.ViewModels;

public class AllergiesViewModel : BaseViewModel
{
    private string _allergies = string.Empty;
    public string Allergies
    {
        get => _allergies;
        set => SetProperty(ref _allergies, value);
    }

    public ICommand SaveCommand { get; }

    public AllergiesViewModel()
    {
        Allergies = "Peanuts";

        SaveCommand = new Command(async () =>
        {
            await Shell.Current.DisplayAlert("Saved", "Allergies updated.", "OK");
        });
    }
}