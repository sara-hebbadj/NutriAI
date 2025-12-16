using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NutriAI.ViewModels;

public partial class BaseViewModel : INotifyPropertyChanged
{
    // Nullable event, matching .NET 9 INotifyPropertyChanged interface
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null!)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(
        ref T backingField,
        T value,
        [CallerMemberName] string propertyName = null!)
    {
        if (EqualityComparer<T>.Default.Equals(backingField, value))
            return false;

        backingField = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
