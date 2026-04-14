// Authorship note:
// Microsoft documentation and MAUI navigation guidance were used
// for route registration and Shell navigation structure in this file.
// The NutriAI navigation routes and page organization were defined by the author.

using NutriAI.Views;

namespace NutriAI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes here
        Routing.RegisterRoute(nameof(RecipeDetailsPage), typeof(RecipeDetailsPage));
        Routing.RegisterRoute(nameof(DietaryPreferencesPage), typeof(DietaryPreferencesPage));
        Routing.RegisterRoute(nameof(HealthGoalsPage), typeof(HealthGoalsPage));
        Routing.RegisterRoute(nameof(AllergiesPage), typeof(AllergiesPage));
        Routing.RegisterRoute(nameof(AccountSettingsPage), typeof(AccountSettingsPage));
        Routing.RegisterRoute(nameof(HelpSupportPage), typeof(HelpSupportPage));
    }
}

