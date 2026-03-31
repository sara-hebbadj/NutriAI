using Microsoft.Extensions.Logging;
using SQLitePCL;
using NutriAI.Services;
using NutriAI.Services.Caching;
using NutriAI.Services.Interactions;
using NutriAI.Services.Recommendation;
using NutriAI.Services.Storage;
using NutriAI.ViewModels;
using NutriAI.Views;

namespace NutriAI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        Batteries.Init();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemibold");
            });

        // REGISTER APP SHELL
        builder.Services.AddSingleton<AppShell>();

        // REGISTER VIEWMODELS
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
        builder.Services.AddTransient<RecipeDetailsViewModel>();

        // REGISTER PAGES
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

        // REGISTER SERVICES
        builder.Services.AddSingleton<IRecipeService, ApiRecipeService>();
        builder.Services.AddSingleton<ICacheService, FileCacheService>();
        builder.Services.AddSingleton<ISavedRecipeStore, FileSavedRecipeStore>();
        builder.Services.AddSingleton<IUserPreferencesStore, FileUserPreferencesStore>();
        builder.Services.AddSingleton<IUserInteractionService, SQLiteUserInteractionService>();
        builder.Services.AddSingleton<IRecommendationService, RecommendationService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}