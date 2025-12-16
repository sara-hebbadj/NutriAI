using NutriAI.Models;
using System.Collections.ObjectModel;

namespace NutriAI.ViewModels;

public class HomeViewModel : BaseViewModel
{
    public ObservableCollection<string> Categories { get; set; }
    public ObservableCollection<Recipe> Recommended { get; set; }

    public HomeViewModel()
    {
        Categories = new ObservableCollection<string>
        {
            "Breakfast", "Lunch", "Dinner", "Snacks", "Vegan"
        };

        Recommended = new ObservableCollection<Recipe>
        {
            new Recipe { Title = "Avocado Toast", ImageUrl="recipe1.png", Duration="10 min", Difficulty="Easy" },
            new Recipe { Title = "Pasta Alfredo", ImageUrl="recipe2.png", Duration="25 min", Difficulty="Medium" }
        };
    }
}
