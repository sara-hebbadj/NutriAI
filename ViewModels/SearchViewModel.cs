using NutriAI.Models;
using System.Collections.ObjectModel;

namespace NutriAI.ViewModels;

public class SearchViewModel : BaseViewModel
{
    public ObservableCollection<Recipe> Results { get; set; }

    public SearchViewModel()
    {
        Results = new ObservableCollection<Recipe>();
    }
}
