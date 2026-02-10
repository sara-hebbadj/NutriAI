using NutriAI.ViewModels;
namespace NutriAI.Views;

public partial class AccountSettingsPage : ContentPage
{
    public AccountSettingsPage(AccountSettingsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is AccountSettingsViewModel vm)
            await vm.RefreshAsync();
    }
}
