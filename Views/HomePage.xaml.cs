using NutriAI.ViewModels;

namespace NutriAI.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        BindingContext = new HomeViewModel();
    }

    private async void OnRecipeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Models.Recipe recipe)
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

    // Flyouts (unchanged)
    private void OnMealTypeTapped(object sender, EventArgs e) =>
        MealTypeFlyout.IsVisible = !MealTypeFlyout.IsVisible;

    private void OnDietTapped(object sender, EventArgs e) =>
        DietFlyout.IsVisible = !DietFlyout.IsVisible;

    private void OnCuisineTapped(object sender, EventArgs e) =>
        CuisineFlyout.IsVisible = !CuisineFlyout.IsVisible;

    private void OnMealTypeSelected(object sender, EventArgs e)
    {
        MealTypeLabel.Text = ((Label)sender).Text;
        MealTypeFlyout.IsVisible = false;
    }

    private void OnDietSelected(object sender, EventArgs e)
    {
        DietLabel.Text = ((Label)sender).Text;
        DietFlyout.IsVisible = false;
    }

    private void OnCuisineSelected(object sender, EventArgs e)
    {
        CuisineLabel.Text = ((Label)sender).Text;
        CuisineFlyout.IsVisible = false;
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
