using NutriAI.Models;
using NutriAI.ViewModels;


namespace NutriAI.Views;

[QueryProperty(nameof(Recipe), "Recipe")]
public partial class RecipeDetailsPage : ContentPage
{
    private readonly RecipeDetailsViewModel _viewModel;
    private Recipe? _pendingRecipe;

    public RecipeDetailsPage(RecipeDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public Recipe? Recipe
    {
        get => _pendingRecipe;
        set
        {
            _pendingRecipe = value;

            // Run async safely, catch exceptions so Android doesn't hard-crash
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    if (_pendingRecipe != null)
                        await _viewModel.SetRecipeAsync(_pendingRecipe);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("RecipeDetailsPage SetRecipeAsync crashed:");
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    await DisplayAlert("Error", ex.Message, "OK");
                }
            });
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

