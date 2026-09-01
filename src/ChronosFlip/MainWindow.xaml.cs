using ChronosFlip.Core.Clocks;
using ChronosFlip.Core.ViewModels;
using ChronosFlip.Services;
using Microsoft.UI.Xaml;

namespace ChronosFlip;

public sealed partial class MainWindow : Window
{
    private readonly ClockService _clock;

    public MainWindow()
    {
        InitializeComponent();

        var ticker = new ClockTicker(new SystemClock());
        var viewModel = new MainClockViewModel();
        viewModel.Attach(ticker);

        _clock = new ClockService(DispatcherQueue, ticker);

        RootGrid.DataContext = viewModel;
        _clock.Start();

        Title = "Chronos Flip";
        AppWindow.Resize(new Windows.Graphics.SizeInt32(672, 340));

        Closed += (_, _) => _clock.Dispose();
    }
}