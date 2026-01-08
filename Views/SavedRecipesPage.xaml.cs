using System;
using Microsoft.Maui.Controls;
using NutriAI.Models;
using NutriAI.ViewModels;

namespace NutriAI.Views;

public partial class SavedRecipesPage : ContentPage
{
    public SavedRecipesPage()
    {
        InitializeComponent();
        BindingContext = new SavedRecipesViewModel();
    }

    private async void OnRecipeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Recipe recipe)
        {
            await Shell.Current.GoToAsync(
                nameof(RecipeDetailsPage),
                new Dictionary<string, object>
                {
                    { "Recipe", recipe }
                });

            ((CollectionView)sender).SelectedItem = null;
        }
    }

    void OnSmallFont(object sender, EventArgs e)
    {
        Application.Current.Resources["BodyFontSize"] =
            Application.Current.Resources["FontSmall"];

        Application.Current.Resources["TitleFontSize"] =
            Application.Current.Resources["TitleSmall"];
    }

    void OnMediumFont(object sender, EventArgs e)
    {
        Application.Current.Resources["BodyFontSize"] =
            Application.Current.Resources["FontMedium"];

        Application.Current.Resources["TitleFontSize"] =
            Application.Current.Resources["TitleMedium"];
    }

    void OnLargeFont(object sender, EventArgs e)
    {
        Application.Current.Resources["BodyFontSize"] =
            Application.Current.Resources["FontLarge"];

        Application.Current.Resources["TitleFontSize"] =
            Application.Current.Resources["TitleLarge"];
    }
}
