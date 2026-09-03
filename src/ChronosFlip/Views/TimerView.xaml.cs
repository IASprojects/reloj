using ChronosFlip.Core.Timers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChronosFlip.Views;

/// <summary>
/// Timer flyout: flip-card MM:SS countdown, duration inputs (editable while
/// Idle), and START/PAUSE/RESET controls. Pure forwarding — all behavior lives
/// in <see cref="TimerViewModel"/> (FR-30). The live tick comes from the shared
/// 1s ticker via the VM, so this view holds no timer of its own (NFR-02).
/// </summary>
public sealed partial class TimerView : UserControl
{
    public TimerView()
    {
        InitializeComponent();
    }

    public TimerViewModel? ViewModel
    {
        get => DataContext as TimerViewModel;
        set => DataContext = value;
    }

    private void OnStartClicked(object sender, RoutedEventArgs e) => ViewModel?.Start();

    private void OnPauseClicked(object sender, RoutedEventArgs e) => ViewModel?.Pause();

    private void OnResetClicked(object sender, RoutedEventArgs e) => ViewModel?.Reset();
}