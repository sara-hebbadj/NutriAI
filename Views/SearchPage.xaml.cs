using NutriAI.ViewModels;

namespace NutriAI.Views
{
    public partial class SearchPage : ContentPage
    {
        public SearchPage(SearchViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
