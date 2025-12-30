using NutriAI.ViewModels;

namespace NutriAI.Views;

public partial class SavedRecipesPage : ContentPage
{
    public SavedRecipesPage(SavedRecipesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
    private async void OnRecipeTapped(object sender, EventArgs e)
    {
        // Navigate to Recipe Details page
        await Shell.Current.GoToAsync("RecipeDetailsPage");

    }
}
