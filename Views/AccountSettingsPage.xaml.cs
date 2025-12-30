using NutriAI.ViewModels;
namespace NutriAI.Views;

public partial class AccountSettingsPage : ContentPage
{
    public AccountSettingsPage()
    {
        InitializeComponent();
        BindingContext = new AccountSettingsViewModel();
    }
}