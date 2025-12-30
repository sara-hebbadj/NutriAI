using System.Windows.Input;

namespace NutriAI.ViewModels;

public class HealthGoalsViewModel : BaseViewModel
{
    private string _goal;
    public string Goal
    {
        get => _goal;
        set => SetProperty(ref _goal, value);
    }

    public ICommand SaveCommand { get; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public HealthGoalsViewModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
        Goal = "Lose weight"; // default or loaded from storage

        SaveCommand = new Command(async () =>
        {
            // Save logic here
            await Shell.Current.DisplayAlert("Saved", "Your health goal has been updated.", "OK");
        });
    }
}