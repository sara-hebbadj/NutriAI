using NutriAI.ViewModels;

namespace NutriAI.Views;

public partial class HomePage : ContentPage
{
    // Strongly-typed ViewModel access
    private HomeViewModel VM => (HomeViewModel)BindingContext;

    public HomePage()
    {
        InitializeComponent();
        BindingContext = new HomeViewModel();
    }

    // ========================
    // SEARCH (NEW – REQUIRED)
    // ========================

    void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        VM.ApplySearch(e.NewTextValue);
    }

    void OnSearchTapped(object sender, EventArgs e)
    {
        // Uses Entry text, NOT CollectionView
        VM.ApplySearch(SearchEntry.Text);
    }

    // ========================
    // NAVIGATION (UNCHANGED)
    // ========================

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

    // ========================
    // FLYOUTS (UNCHANGED)
    // ========================

    private void OnMealTypeTapped(object sender, EventArgs e) =>
        MealTypeFlyout.IsVisible = !MealTypeFlyout.IsVisible;

    private void OnDietTapped(object sender, EventArgs e) =>
        DietFlyout.IsVisible = !DietFlyout.IsVisible;

    private void OnCuisineTapped(object sender, EventArgs e) =>
        CuisineFlyout.IsVisible = !CuisineFlyout.IsVisible;

    private void OnMealTypeSelected(object sender, EventArgs e)
    {
        if (sender is Label label && label.Text != null)
        {
            MealTypeLabel.Text = label.Text;
            VM.SelectedMealType = label.Text;
        }

        MealTypeFlyout.IsVisible = false;
    }

    private void OnDietSelected(object sender, EventArgs e)
    {
        if (sender is Label label && label.Text != null)
        {
            DietLabel.Text = label.Text;
            VM.SelectedDiet = label.Text;
        }

        DietFlyout.IsVisible = false;
    }

    private void OnCuisineSelected(object sender, EventArgs e)
    {
        if (sender is Label label && label.Text != null)
        {
            CuisineLabel.Text = label.Text;
            VM.SelectedCuisine = label.Text;
        }

        CuisineFlyout.IsVisible = false;
    }

    // ========================
    // FONT SIZE (UNCHANGED)
    // ========================

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
