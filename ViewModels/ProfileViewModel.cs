using System.Windows.Input;
using NutriAI.Services;
using NutriAI.Services.Storage;
using NutriAI.Services.Interactions;

namespace NutriAI.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private readonly IRecipeService _recipeService;
    private readonly IUserPreferencesStore _preferencesStore;
    private readonly IUserInteractionService _interactionService;

    private string _name = "User";
    public string Name
    {
        get => _name;
        private set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    private string _goalSummary = "No goal set yet";
    public string GoalSummary
    {
        get => _goalSummary;
        private set
        {
            _goalSummary = value;
            OnPropertyChanged();
        }
    }

    private int _recipesSaved;
    public int RecipesSaved
    {
        get => _recipesSaved;
        private set
        {
            _recipesSaved = value;
            OnPropertyChanged();
        }
    }

    private int _recipesViewed;
    public int RecipesViewed
    {
        get => _recipesViewed;
        private set
        {
            _recipesViewed = value;
            OnPropertyChanged();
        }
    }

    public ICommand OpenPageCommand { get; }
    public double WeightLost => 0;

    private int _mealsCooked;
    public int MealsCooked
    {
        get => _mealsCooked;
        private set { _mealsCooked = value; OnPropertyChanged(); }
    }

    public ProfileViewModel(
        IRecipeService recipeService,
        IUserPreferencesStore preferencesStore,
        IUserInteractionService interactionService)
    {
        _recipeService = recipeService;
        _preferencesStore = preferencesStore;
        _interactionService = interactionService;

        OpenPageCommand = new Command<string>(async (page) =>
        {
            await Shell.Current.GoToAsync(page);
        });
    }

    //  CALLED every time Profile page appears
    public async Task RefreshAsync()
    {
        //  SAME source as SavedRecipesPage
        RecipesSaved = _recipeService.GetSavedRecipes().Count;

        //  SQLite interaction data
        var interactions = await _interactionService.GetAllAsync();
        RecipesViewed = interactions.Sum(i => i.ViewCount);

        //  Preferences
        var preferences = await _preferencesStore.LoadAsync();
        GoalSummary = string.IsNullOrEmpty(preferences.Goal)
            ? "No goal set yet"
            : $"{preferences.Goal} • {preferences.DailyCalorieTarget ?? 0} cal/day";
        Name = string.IsNullOrWhiteSpace(preferences.Name)
            ? "User"
            : preferences.Name;

        //  Meals Cooked
        MealsCooked = interactions.Sum(i => i.CookCount);


    }
}
