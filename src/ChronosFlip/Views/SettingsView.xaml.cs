using System.Threading;
using ChronosFlip.Core.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChronosFlip.Views;

public sealed partial class SettingsView : UserControl
{
    private readonly CancellationTokenSource _debounce = new();
    private static readonly TimeSpan DebounceDelay = TimeSpan.FromMilliseconds(400);

    public SettingsView()
    {
        InitializeComponent();
    }

    public SettingsViewModel? ViewModel
    {
        get => DataContext as SettingsViewModel;
        set
        {
            if (DataContext is SettingsViewModel old)
            {
                old.PropertyChanged -= OnViewModelPropertyChanged;
            }
            DataContext = value;
            if (value is not null)
            {
                value.PropertyChanged += OnViewModelPropertyChanged;
            }
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        _debounce.Cancel();
        var token = _debounce.Token;
        _ = DebouncedSaveAsync(ViewModel, token);
    }

    private static async Task DebouncedSaveAsync(SettingsViewModel vm, CancellationToken token)
    {
        try
        {
            await Task.Delay(DebounceDelay, token);
            vm.Save();
        }
        catch (TaskCanceledException)
        {
        }
    }
}
