using NutriAI.ViewModels;

namespace NutriAI.Views;

public partial class SavedRecipesPage : ContentPage
{
    public SavedRecipesPage(SavedRecipesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
