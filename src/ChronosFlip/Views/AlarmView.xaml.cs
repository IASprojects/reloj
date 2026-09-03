using ChronosFlip.Core.Alarms;
using ChronosFlip.Core.WorldClock;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChronosFlip.Views;

/// <summary>
/// Alarms flyout: lists scheduled alarms with an enable toggle, a DELETE
/// affordance, a STOP button per ringing alarm (FR-22), and a create row that
/// builds a zone-local wall time into an absolute instant via
/// <see cref="AlarmViewModel.AddAlarmAt"/>. Pure forwarding — the VM owns all
/// alarm logic and persistence signaling.
/// </summary>
public sealed partial class AlarmView : UserControl
{
    public AlarmView()
    {
        InitializeComponent();
    }

    public AlarmViewModel? ViewModel
    {
        get => DataContext as AlarmViewModel;
        set => DataContext = value;
    }

    public void SetZones(IEnumerable<ClockZone> zones)
    {
        ArgumentNullException.ThrowIfNull(zones);
        ZoneSelector.ItemsSource = zones.ToList();
        ZoneSelector.SelectedIndex = 0;
    }

    private void OnDismissAlarmClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Alarm alarm })
        {
            ViewModel?.Dismiss(alarm.Id);
        }
    }

    private void OnAlarmToggleChanged(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleSwitch { Tag: Alarm alarm } toggle)
        {
            ViewModel?.SetEnabled(alarm.Id, toggle.IsOn);
        }
    }

    private void OnDeleteAlarmClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: Alarm alarm })
        {
            ViewModel?.RemoveAlarm(alarm.Id);
        }
    }

    private void OnAddAlarmClicked(object sender, RoutedEventArgs e)
    {
        var viewModel = ViewModel;
        if (viewModel is null || ZoneSelector.SelectedItem is not ClockZone zone)
        {
            return;
        }

        var wallDate = AlarmDatePicker.Date;
        var wallTime = AlarmTimePicker.Time;
        var wall = wallDate.Date + wallTime;

        if (viewModel.AddAlarmAt(zone.TimeZoneId, wall, zone.Label))
        {
            AlarmDatePicker.Date = DateTimeOffset.Now.Date;
            AlarmTimePicker.Time = TimeSpan.FromHours(DateTime.Now.Hour + 1);
        }
    }
}