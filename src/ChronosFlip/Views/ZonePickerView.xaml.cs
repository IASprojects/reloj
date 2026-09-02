using System.Collections.ObjectModel;
using ChronosFlip.Core.WorldClock;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ChronosFlip.Views;

/// <summary>
/// Zone picker flyout: lists zones currently shown (with a remove affordance)
/// and searchable zones to add. Raises add/remove requests; the host decides
/// how to apply and persist them (MVVM — no logic here beyond forwarding).
/// </summary>
public sealed partial class ZonePickerView : UserControl
{
    public ZonePickerView()
    {
        InitializeComponent();
    }

    public event EventHandler<ClockZone>? AddRequested;

    public event EventHandler<string>? RemoveRequested;

    public ZonePickerViewModel? ViewModel
    {
        get => DataContext as ZonePickerViewModel;
        set => DataContext = value;
    }

    public void SetTray(ObservableCollection<WorldClockCardViewModel> cards)
    {
        ArgumentNullException.ThrowIfNull(cards);
        TrayList.ItemsSource = cards;
    }

    private void OnAddZoneClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ClockZone zone })
        {
            AddRequested?.Invoke(this, zone);
        }
    }

    private void OnRemoveZoneClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string zoneId })
        {
            RemoveRequested?.Invoke(this, zoneId);
        }
    }
}