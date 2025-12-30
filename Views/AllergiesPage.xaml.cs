using NutriAI.ViewModels;
namespace NutriAI.Views;

public partial class AllergiesPage : ContentPage
{
    public AllergiesPage()
    {
        InitializeComponent();
        BindingContext = new AllergiesViewModel();
    }
}