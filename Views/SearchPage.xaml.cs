using NutriAI.Models;
using NutriAI.Services;
using System.Collections.ObjectModel;

namespace NutriAI.Views;

public partial class SearchPage : ContentPage
{
    // ========================
    // SEARCH DATA
    // ========================
    private readonly IRecipeService _recipeService = ServiceLocator.RecipeService;
    private ObservableCollection<Recipe> _allRecipes = new();
    private readonly ObservableCollection<Recipe> _filteredRecipes = new();

    public SearchPage()
    {
        InitializeComponent();

        _allRecipes = _recipeService.GetAllRecipes();
        ResultsCollection.ItemsSource = _filteredRecipes;

        // Start EMPTY
        _filteredRecipes.Clear();

    }

    // ========================
    // SEARCH LOGIC
    // ========================

    void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplySearch(e.NewTextValue);
    }

    void OnSearchTapped(object sender, EventArgs e)
    {
        ApplySearch(SearchEntry.Text);
    }

    void ApplySearch(string? query)
    {
        _filteredRecipes.Clear();

        if (string.IsNullOrWhiteSpace(query))
            return; // 👈 nothing shown until typing

        string q = query.ToLowerInvariant();

        var results = _allRecipes.Where(r =>
            (!string.IsNullOrWhiteSpace(r.Title) &&
                r.Title.ToLowerInvariant().Contains(q)) ||

            (r.Ingredients != null &&
                r.Ingredients.Any(i =>
                    !string.IsNullOrWhiteSpace(i) &&
                    i.ToLowerInvariant().Contains(q))) ||

            (!string.IsNullOrWhiteSpace(r.MealType) &&
                r.MealType.ToLowerInvariant().Contains(q)) ||

            (!string.IsNullOrWhiteSpace(r.Diet) &&
                r.Diet.ToLowerInvariant().Contains(q)) ||

            (!string.IsNullOrWhiteSpace(r.Cuisine) &&
                r.Cuisine.ToLowerInvariant().Contains(q))
        );

        foreach (var r in results)
            _filteredRecipes.Add(r);
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


    // ========================
    // FLYOUT TOGGLE HELPERS
    // ========================

    void CloseAllFlyouts()
    {
        MealTypeFlyout.IsVisible = false;
        DietFlyout.IsVisible = false;
        CuisineFlyout.IsVisible = false;
    }

    void OnMealTypeTapped(object sender, EventArgs e)
    {
        bool open = !MealTypeFlyout.IsVisible;
        CloseAllFlyouts();
        MealTypeFlyout.IsVisible = open;
    }

    void OnDietTapped(object sender, EventArgs e)
    {
        bool open = !DietFlyout.IsVisible;
        CloseAllFlyouts();
        DietFlyout.IsVisible = open;
    }

    void OnCuisineTapped(object sender, EventArgs e)
    {
        bool open = !CuisineFlyout.IsVisible;
        CloseAllFlyouts();
        CuisineFlyout.IsVisible = open;
    }

    // ========================
    // SELECTION HANDLERS
    // ========================

    void OnMealTypeSelected(object sender, EventArgs e)
    {
        if (sender is Label label)
            MealTypeLabel.Text = label.Text;

        CloseAllFlyouts();
    }

    void OnDietSelected(object sender, EventArgs e)
    {
        if (sender is Label label)
            DietLabel.Text = label.Text;

        CloseAllFlyouts();
    }

    void OnCuisineSelected(object sender, EventArgs e)
    {
        if (sender is Label label)
            CuisineLabel.Text = label.Text;

        CloseAllFlyouts();
    }

    // ========================
    // FONT SIZE CONTROLS
    // ========================

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

    void OnSmallFont(object sender, EventArgs e)
    {
        Application.Current.Resources["BodyFontSize"] =
            Application.Current.Resources["FontSmall"];

        Application.Current.Resources["TitleFontSize"] =
            Application.Current.Resources["TitleSmall"];
    }
}
