using NutriAI.ViewModels;

namespace NutriAI.Views;

public partial class DietaryPreferencesPage : ContentPage
{
    public DietaryPreferencesPage(DietaryPreferencesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
