using NutriAI.ViewModels;

namespace NutriAI.Views;

public partial class HomePage : ContentPage
{
    // Strongly-typed ViewModel access
    private HomeViewModel VM => (HomeViewModel)BindingContext;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ((HomeViewModel)BindingContext).InitializeAsync();
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

    void CloseAllFlyouts()
    {
        MealTypeFlyout.IsVisible = false;
        DietFlyout.IsVisible = false;
        CuisineFlyout.IsVisible = false;
    }

    void OnMealTypeTapped(object sender, EventArgs e)
    {
        CloseAllFlyouts();
        MealTypeFlyout.IsVisible = true;
    }

    void OnDietTapped(object sender, EventArgs e)
    {
        CloseAllFlyouts();
        DietFlyout.IsVisible = true;
    }

    void OnCuisineTapped(object sender, EventArgs e)
    {
        CloseAllFlyouts();
        CuisineFlyout.IsVisible = true;
    }

    // ========================
    // SELECTION HANDLERS
    // ========================

    void OnMealTypeSelected(object sender, EventArgs e)
    {
        if (sender is Label label)
        {
            var value = label.Text.ToLowerInvariant();

            VM.SelectedMealType =
                VM.SelectedMealType == value ? null : value;

            MealTypeLabel.Text =
                VM.SelectedMealType == null ? "Meal Type" : label.Text;

            VM.ApplyFilters();
        }

        CloseAllFlyouts();
    }

    void OnDietSelected(object sender, EventArgs e)
    {
        if (sender is Label label)
        {
            var value = label.Text.ToLowerInvariant();

            VM.SelectedDiet =
                VM.SelectedDiet == value ? null : value;

            DietLabel.Text =
                VM.SelectedDiet == null ? "Diet" : label.Text;

            VM.ApplyFilters();
        }

        CloseAllFlyouts();
    }

    void OnCuisineSelected(object sender, EventArgs e)
    {
        if (sender is Label label)
        {
            var value = label.Text.ToLowerInvariant();

            VM.SelectedCuisine =
                VM.SelectedCuisine == value ? null : value;

            CuisineLabel.Text =
                VM.SelectedCuisine == null ? "Cuisine" : label.Text;

            VM.ApplyFilters();
        }

        CloseAllFlyouts();
    }


    // ========================
    // CLEAR FILTERS
    // ========================

    private void OnClearFiltersTapped(object sender, EventArgs e)
    {
        VM.ClearFilters();

        MealTypeLabel.Text = "Meal Type";
        DietLabel.Text = "Diet";
        CuisineLabel.Text = "Cuisine";

        CloseAllFlyouts();
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