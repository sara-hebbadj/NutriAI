using NutriAI.Models;
using NutriAI.ViewModels;

namespace NutriAI.Views;

[QueryProperty(nameof(Recipe), "Recipe")]
public partial class RecipeDetailsPage : ContentPage
{
    private Recipe _recipe;

    public Recipe Recipe
    {
        get => _recipe;
        set
        {
            _recipe = value;
            BindingContext = new RecipeDetailsViewModel(_recipe);
        }
    }

    public RecipeDetailsPage()
    {
        InitializeComponent();
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
