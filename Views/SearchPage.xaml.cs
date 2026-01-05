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


