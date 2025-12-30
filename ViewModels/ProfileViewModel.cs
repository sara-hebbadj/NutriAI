using System.Windows.Input;

namespace NutriAI.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    public string Name => "Sara Hebbadj";
    public ICommand OpenPageCommand { get; }

    public ProfileViewModel()
    {
        OpenPageCommand = new Command<string>(async (page) =>
        {
            await Shell.Current.GoToAsync(page);
        });
    }
}


