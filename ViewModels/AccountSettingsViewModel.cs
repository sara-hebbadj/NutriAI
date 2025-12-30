using System.Windows.Input;

namespace NutriAI.ViewModels;

public class AccountSettingsViewModel : BaseViewModel
{
    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public ICommand UpdateCommand { get; }

    public AccountSettingsViewModel()
    {
        Email = "sara@example.com";

        UpdateCommand = new Command(async () =>
        {
            await Shell.Current.DisplayAlert("Updated", "Account settings saved.", "OK");
        });
    }
}