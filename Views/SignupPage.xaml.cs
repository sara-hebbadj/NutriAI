using NutriAI.ViewModels;

namespace NutriAI.Views;

public partial class SignupPage : ContentPage
{
    public SignupPage(SignupViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
