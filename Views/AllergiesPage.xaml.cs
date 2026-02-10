using NutriAI.ViewModels;

namespace NutriAI.Views;

public partial class AllergiesPage : ContentPage
{
    public AllergiesPage(AllergiesViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
