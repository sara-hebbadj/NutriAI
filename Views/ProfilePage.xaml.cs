using Microsoft.Maui.Controls;
using NutriAI.ViewModels;

namespace NutriAI.Views
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePage(ProfileViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}