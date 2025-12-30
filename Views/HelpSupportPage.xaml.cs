using NutriAI.ViewModels;
namespace NutriAI.Views;

public partial class HelpSupportPage : ContentPage
{
    public HelpSupportPage()
    {
        InitializeComponent();
        BindingContext = new HelpSupportViewModel();
    }
}