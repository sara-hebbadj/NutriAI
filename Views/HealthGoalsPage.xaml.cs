using NutriAI.ViewModels;

namespace NutriAI.Views;

public partial class HealthGoalsPage : ContentPage
{
    public HealthGoalsPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.HealthGoalsViewModel();
    }
}