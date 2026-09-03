using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChronosFlip.Core.Timers;

/// <summary>
/// View-facing layer over <see cref="CountdownTimer"/> (FR-30): observable
/// minute/second inputs that apply a new duration while Idle, state-driven
/// control booleans, and a single-shot <see cref="Expired"/> passthrough so the
/// shell can sound the chime and the card can react (FR-32). Time advances only
/// through <see cref="Evaluate(DateTimeOffset)"/> from the shared tick (NFR-02).
/// </summary>
public partial class TimerViewModel : ObservableObject
{
    private readonly CountdownTimer _timer;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanStart))]
    [NotifyPropertyChangedFor(nameof(CanPause))]
    [NotifyPropertyChangedFor(nameof(CanReset))]
    [NotifyPropertyChangedFor(nameof(CanEditDuration))]
    [NotifyPropertyChangedFor(nameof(IsExpired))]
    private TimerState _state;

    [ObservableProperty]
    private int _inputMinutes;

    [ObservableProperty]
    private int _inputSeconds;

    public TimerViewModel(CountdownTimer? timer = null)
    {
        _timer = timer ?? new CountdownTimer();
        _timer.PropertyChanged += OnTimerPropertyChanged;
        _timer.Expired += (_, _) => Expired?.Invoke(this, EventArgs.Empty);
        State = _timer.State;
        SyncInputs();
    }

    /// <summary>Underlying countdown engine; digits bind to its computed parts.</summary>
    public CountdownTimer Timer => _timer;

    /// <summary>Forwarded from the engine exactly once per expiry (FR-32).</summary>
    public event EventHandler? Expired;

    public bool CanStart => State is TimerState.Idle or TimerState.Paused;

    public bool CanPause => State == TimerState.Running;

    public bool CanReset => State is TimerState.Running or TimerState.Paused or TimerState.Expired;

    /// <summary>True only while Idle, so the duration inputs stay editable.</summary>
    public bool CanEditDuration => State == TimerState.Idle;

    public bool IsExpired => State == TimerState.Expired;

    partial void OnInputMinutesChanged(int value) => ApplyInputs();

    partial void OnInputSecondsChanged(int value) => ApplyInputs();

    /// <summary>Restores a persisted last duration (FR-33); clamps to the usable range.</summary>
    public void RestoreDuration(int seconds)
    {
        var clamped = Math.Clamp(seconds, 1, (int)CountdownTimer.MaxSeconds);
        _timer.SetDuration(TimeSpan.FromSeconds(clamped));
        SyncInputs();
    }

    public void Start()
    {
        if (CanStart)
        {
            _timer.Start();
        }
    }

    public void Pause()
    {
        if (CanPause)
        {
            _timer.Pause();
        }
    }

    public void Reset()
    {
        if (CanReset)
        {
            _timer.Reset();
        }
    }

    /// <summary>Advances the countdown with the shared tick's instant.</summary>
    public void Evaluate(DateTimeOffset now) => _timer.Evaluate(now);

    private void ApplyInputs()
    {
        if (State != TimerState.Idle)
        {
            return;
        }

        var totalSeconds = Math.Clamp(InputMinutes, 0, 99) * 60 + Math.Clamp(InputSeconds, 0, 59);
        if (totalSeconds < 1)
        {
            return;
        }

        _timer.SetDuration(TimeSpan.FromSeconds(totalSeconds));
        InputMinutes = totalSeconds / 60;
        InputSeconds = totalSeconds % 60;
    }

    private void SyncInputs()
    {
        var totalSeconds = (int)_timer.Duration.TotalSeconds;
        InputMinutes = totalSeconds / 60;
        InputSeconds = totalSeconds % 60;
    }

    private void OnTimerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CountdownTimer.State))
        {
            State = _timer.State;
        }
    }
}