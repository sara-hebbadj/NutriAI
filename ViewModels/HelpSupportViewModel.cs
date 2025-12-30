using System.Windows.Input;

namespace NutriAI.ViewModels;

public class HelpSupportViewModel : BaseViewModel
{
    public ICommand ContactSupportCommand { get; }

    public HelpSupportViewModel()
    {
        ContactSupportCommand = new Command(async () =>
        {
            await Shell.Current.DisplayAlert("Support", "Support request sent.", "OK");
        });
    }
}