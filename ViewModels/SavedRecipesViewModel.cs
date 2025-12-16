using NutriAI.Models;
using System.Collections.ObjectModel;

namespace NutriAI.ViewModels;

public class SavedRecipesViewModel : BaseViewModel
{
    public ObservableCollection<Recipe> Saved { get; set; }

    public SavedRecipesViewModel()
    {
        Saved = new ObservableCollection<Recipe>();
    }
}
