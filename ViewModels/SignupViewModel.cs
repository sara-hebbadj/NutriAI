using System.Windows.Input;

namespace NutriAI.ViewModels;

public class SignupViewModel : BaseViewModel
{
    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public ICommand SignupCommand { get; }

    public SignupViewModel()
    {
        SignupCommand = new Command(OnSignup);
    }

    private async void OnSignup()
    {
        await Shell.Current.DisplayAlert(
            "Signup",
            "Account created successfully!",
            "OK"
        );
    }
}
