// Authorship note:
// This file was written by the author for a simple view-model.
// External help was limited to standard .NET MAUI / MVVM syntax examples and minor boilerplate support from copilot.
// The page purpose, bound fields, and its role in the application were decided by the author.

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NutriAI.ViewModels;

public partial class BaseViewModel : INotifyPropertyChanged
{
    
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
