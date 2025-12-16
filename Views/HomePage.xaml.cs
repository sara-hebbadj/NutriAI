using NutriAI.ViewModels;

namespace NutriAI.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
