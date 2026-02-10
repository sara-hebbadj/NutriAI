using NutriAI.ViewModels;

namespace NutriAI.Views;

public partial class HealthGoalsPage : ContentPage
{
    public HealthGoalsPage(HealthGoalsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
