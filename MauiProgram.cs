using Microsoft.Extensions.Logging;
using NutriAI.Views;
using NutriAI.ViewModels;

namespace NutriAI;

using NutriAI.Views;
using NutriAI.ViewModels;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemibold");
            });

        // REGISTER APP SHELL
        builder.Services.AddSingleton<AppShell>();

        // ----------------------------------------
        // REGISTER VIEWMODELS
        // ----------------------------------------
        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<SearchViewModel>();
        builder.Services.AddSingleton<SavedRecipesViewModel>();
        builder.Services.AddSingleton<ProfileViewModel>();
        builder.Services.AddSingleton<SignupViewModel>();
        builder.Services.AddSingleton<HealthGoalsViewModel>();
        builder.Services.AddSingleton<HelpSupportViewModel>();
        builder.Services.AddSingleton<DietaryPreferencesViewModel>();
        builder.Services.AddSingleton<AllergiesViewModel>();
        builder.Services.AddSingleton<AccountSettingsViewModel>();

        // ----------------------------------------
        // REGISTER PAGES
        // ----------------------------------------
        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<SearchPage>();
        builder.Services.AddSingleton<SavedRecipesPage>();
        builder.Services.AddSingleton<ProfilePage>();
        builder.Services.AddSingleton<SignupPage>();
        builder.Services.AddTransient<RecipeDetailsPage>();
        builder.Services.AddSingleton<HealthGoalsPage>();
        builder.Services.AddSingleton<DietaryPreferencesPage>();
        builder.Services.AddSingleton<HelpSupportPage>();
        builder.Services.AddSingleton<AccountSettingsPage>();
        builder.Services.AddSingleton<AllergiesPage>();

        // Recipe details should be transient, not global
        builder.Services.AddTransient<RecipeDetailsPage>();


#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
