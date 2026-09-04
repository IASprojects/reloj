using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChronosFlip.Core.Navigation;

public enum MainNavigationPage
{
    Clock,
    Alarm,
    Timer,
}

/// <summary>
/// Navigation state for the widget dashboard (feature 08). <see cref="SelectedPage"/>
/// drives the shared content region and the per-page selected flags; re-selecting
/// the current page is a no-op. Pure state — no timers, no events.
/// </summary>
public partial class MainNavigationViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsClockSelected))]
    [NotifyPropertyChangedFor(nameof(IsAlarmSelected))]
    [NotifyPropertyChangedFor(nameof(IsTimerSelected))]
    private MainNavigationPage _selectedPage = MainNavigationPage.Clock;

    public bool IsClockSelected => SelectedPage == MainNavigationPage.Clock;

    public bool IsAlarmSelected => SelectedPage == MainNavigationPage.Alarm;

    public bool IsTimerSelected => SelectedPage == MainNavigationPage.Timer;

    /// <summary>Typed command parameters for the nav-rail buttons (feature 08).</summary>
    public MainNavigationPage ClockPage => MainNavigationPage.Clock;

    public MainNavigationPage AlarmPage => MainNavigationPage.Alarm;

    public MainNavigationPage TimerPage => MainNavigationPage.Timer;

    public RelayCommand<MainNavigationPage> SelectCommand { get; }

    public MainNavigationViewModel()
    {
        SelectCommand = new RelayCommand<MainNavigationPage>(Select);
    }

    public void Select(MainNavigationPage page)
    {
        if (SelectedPage == page)
        {
            return;
        }

        SelectedPage = page;
    }
}