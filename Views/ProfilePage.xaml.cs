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
        void OnSmallFont(object sender, EventArgs e)
        {
            Application.Current.Resources["BodyFontSize"] =
                Application.Current.Resources["FontSmall"];

            Application.Current.Resources["TitleFontSize"] =
                Application.Current.Resources["TitleSmall"];
        }

        void OnMediumFont(object sender, EventArgs e)
        {
            Application.Current.Resources["BodyFontSize"] =
                Application.Current.Resources["FontMedium"];

            Application.Current.Resources["TitleFontSize"] =
                Application.Current.Resources["TitleMedium"];
        }

        void OnLargeFont(object sender, EventArgs e)
        {
            Application.Current.Resources["BodyFontSize"] =
                Application.Current.Resources["FontLarge"];

            Application.Current.Resources["TitleFontSize"] =
                Application.Current.Resources["TitleLarge"];
        }
    }

}