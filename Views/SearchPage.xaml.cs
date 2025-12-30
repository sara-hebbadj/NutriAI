using NutriAI.ViewModels;

namespace NutriAI.Views;

public partial class SearchPage : ContentPage
{
    public SearchPage()
    {
        InitializeComponent();
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
}


