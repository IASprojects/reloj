using ChronosFlip.Core.Settings;
using ChronosFlip.Core.ViewModels;
using ChronosFlip.Core.WindowModes;

namespace ChronosFlip.Tests.WindowModes;

public sealed class WindowModeViewModelTests : IDisposable
{
    private static readonly string? s_baseTemp =
        Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "ChronosFlip.Tests")).FullName;

    private readonly string _directory;

    public WindowModeViewModelTests()
    {
        _directory = Path.Combine(s_baseTemp!, Guid.NewGuid().ToString("N"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    private static (WindowModeViewModel Vm, FakeWindowModeService Service, SettingsViewModel Settings, SettingsStore Store) Create()
    {
        var store = new SettingsStore(
            Path.Combine(Path.GetTempPath(), "ChronosFlip.Tests", Guid.NewGuid().ToString("N")));
        var settings = new SettingsViewModel(store);
        settings.Load();
        var service = new FakeWindowModeService();
        var vm = new WindowModeViewModel(service, settings);
        return (vm, service, settings, store);
    }

    [Fact]
    public void Enter_InvokesService_AndFlipsState()
    {
        var (vm, service, _, _) = Create();

        vm.EnterFullScreen();

        Assert.True(vm.IsFullScreen);
        Assert.True(service.IsFullScreen);
        Assert.Contains("EnterFullScreen", service.Calls);
    }

    [Fact]
    public void Enter_WhenAlreadyFullScreen_IsNoOp()
    {
        var (vm, service, _, _) = Create();
        vm.EnterFullScreen();

        vm.EnterFullScreen();

        Assert.Single(service.Calls, call => call == "EnterFullScreen");
    }

    [Fact]
    public void Exit_WhenNotFullScreen_IsNoOp()
    {
        var (vm, service, _, _) = Create();

        vm.ExitFullScreen();

        Assert.False(vm.IsFullScreen);
        Assert.DoesNotContain("ExitFullScreen", service.Calls);
    }

    [Fact]
    public void Exit_WhenFullScreen_RestoresAndReappliesTopmost()
    {
        var (vm, service, settings, _) = Create();
        settings.PinToTop = true;
        vm.EnterFullScreen();
        service.Calls.Clear();

        vm.ExitFullScreen();

        Assert.False(vm.IsFullScreen);
        Assert.False(service.IsFullScreen);
        Assert.Contains("ExitFullScreen", service.Calls);
        Assert.Contains("SetTopmost(True)", service.Calls);
    }

    [Fact]
    public void ToggleFullScreen_TogglesInAndOut()
    {
        var (vm, service, _, _) = Create();

        vm.ToggleFullScreen();
        Assert.True(vm.IsFullScreen);

        vm.ToggleFullScreen();
        Assert.False(vm.IsFullScreen);
        Assert.False(service.IsFullScreen);
    }

    [Fact]
    public void Enter_WhenServiceThrows_StaysInWidgetMode()
    {
        var (vm, service, _, _) = Create();
        service.ThrowOnEnter = true;

        vm.EnterFullScreen();

        Assert.False(vm.IsFullScreen);
        Assert.False(service.IsFullScreen);
    }

    [Fact]
    public void Exit_WhenServiceThrows_KeepsWindowFullscreen()
    {
        var (vm, service, _, _) = Create();
        vm.EnterFullScreen();
        service.ThrowOnExit = true;

        vm.ExitFullScreen();

        Assert.True(vm.IsFullScreen);
        Assert.True(service.IsFullScreen);
    }

    [Fact]
    public void RequestExit_WhenFullScreen_Exits()
    {
        var (vm, service, _, _) = Create();
        vm.EnterFullScreen();

        vm.RequestExit();

        Assert.False(vm.IsFullScreen);
        Assert.False(service.IsFullScreen);
    }

    [Fact]
    public void RequestExit_WhenNotFullScreen_IsNoOp()
    {
        var (vm, service, _, _) = Create();

        vm.RequestExit();

        Assert.DoesNotContain("ExitFullScreen", service.Calls);
    }

    [Fact]
    public void TogglePin_SetsTopmost_AndPersists()
    {
        var (vm, service, settings, store) = Create();

        vm.TogglePin();
        settings.Save();
        var reloaded = store.Load();

        Assert.True(service.LastTopmost);
        Assert.True(settings.PinToTop);
        Assert.True(reloaded.PinToTop);
    }

    [Fact]
    public void TogglePin_SecondToggle_Unpins()
    {
        var (vm, service, _, _) = Create();
        vm.TogglePin();

        vm.TogglePin();

        Assert.False(service.LastTopmost);
        Assert.False(vm.IsPinActive);
    }

    [Fact]
    public void IsPinActive_Setter_WritesThroughToServiceAndSettings()
    {
        var (vm, service, settings, _) = Create();

        vm.IsPinActive = true;

        Assert.True(service.LastTopmost);
        Assert.True(settings.PinToTop);
        Assert.True(vm.IsPinActive);
    }

    [Fact]
    public void IsPinActive_Setter_WhenUnchanged_DoesNotToggle()
    {
        var (vm, service, _, _) = Create();
        vm.IsPinActive = false;

        Assert.Empty(service.Calls);
    }

    [Fact]
    public void TogglePin_WhenServiceThrows_DoesNotPersist()
    {
        var (vm, service, settings, _) = Create();
        service.ThrowOnTopmost = true;

        vm.TogglePin();

        Assert.False(settings.PinToTop);
        Assert.False(vm.IsPinActive);
    }

    [Fact]
    public void IsPinActive_Setter_WhenServiceThrows_MirrorsUnchangedState()
    {
        var (vm, service, settings, _) = Create();
        service.ThrowOnTopmost = true;
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        vm.IsPinActive = true;

        Assert.False(vm.IsPinActive);
        Assert.Contains(nameof(WindowModeViewModel.IsPinActive), raised);
    }

    [Fact]
    public void IsPinActive_RaisesPropertyChanged_WhenSettingsChangeElsewhere()
    {
        var (vm, _, settings, _) = Create();
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        settings.PinToTop = true;

        Assert.Contains(nameof(WindowModeViewModel.IsPinActive), raised);
    }
}