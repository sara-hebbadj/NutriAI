using NutriAI.ViewModels;

namespace NutriAI.Views;

public partial class DietaryPreferencesPage : ContentPage
{
    public DietaryPreferencesPage()
    {
        InitializeComponent();
        BindingContext = new DietaryPreferencesViewModel();
    }
}