using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace HnVue.UI.Components.Common;

/// <summary>
/// Base class for ViewModels implementing INotifyPropertyChanged.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    private readonly ConcurrentDictionary<string, PropertyChangedEventArgs> _argCache = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event for the specified property.
    /// </summary>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        var args = _argCache.GetOrAdd(propertyName, p => new PropertyChangedEventArgs(p));
        PropertyChanged?.Invoke(this, args);
    }

    /// <summary>
    /// Sets the property value and raises PropertyChanged if changed.
    /// </summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName!);
        return true;
    }
}

/// <summary>
/// Simple implementation of ICommand for ViewModels.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

/// <summary>
/// Generic RelayCommand with typed parameter.
/// </summary>
public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;

    public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        if (parameter is not T typed)
            return _canExecute is null;

        return _canExecute?.Invoke(typed) ?? true;
    }

    public void Execute(object? parameter)
    {
        if (parameter is T typed || parameter is null && default(T) is null)
            _execute((T?)parameter);
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
