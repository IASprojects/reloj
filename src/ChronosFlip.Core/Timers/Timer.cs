using CommunityToolkit.Mvvm.ComponentModel;

namespace ChronosFlip.Core.Timers;

public enum TimerState
{
    Idle,
    Running,
    Paused,
    Expired,
}

/// <summary>
/// Pure countdown state machine: Idle → Running ⇄ Paused → Expired →(Reset)→
/// Idle (FR-30). Clock-free: the shared 1s tick drives it through
/// <see cref="Evaluate(DateTimeOffset)"/>, so no internal timer exists (NFR-02).
/// Drift-free by design: while running, <see cref="Remaining"/> is recomputed as
/// <c>EndsAt − now</c> from an absolute instant, so throttled/missed ticks or a
/// paused window can never accumulate error (FR-30 accuracy AC).
/// Expiry raises <see cref="Expired"/> exactly once (single-shot) and the timer
/// stays sticky in <see cref="TimerState.Expired"/> at zero until
/// <see cref="Reset"/> (user-chosen alarm-style UX).
/// </summary>
public partial class CountdownTimer : ObservableObject
{
    public const long MaxSeconds = 99 * 60 + 59; // 99:59 fits MM:SS flip digits

    [ObservableProperty]
    private TimerState _state = TimerState.Idle;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RemainingMinutes))]
    [NotifyPropertyChangedFor(nameof(RemainingSeconds))]
    [NotifyPropertyChangedFor(nameof(RemainingText))]
    private TimeSpan _remaining;

    /// <summary>Zero-padded whole countdown minutes as "<c>MM</c>" (clamped to 99:59).</summary>
    public string RemainingMinutes => Math.Clamp((int)Math.Floor(Remaining.TotalSeconds / 60), 0, 99).ToString("00");

    /// <summary>Zero-padded countdown seconds (0-59).</summary>
    public string RemainingSeconds => ((int)Math.Floor(Remaining.TotalSeconds) % 60).ToString("00");

    /// <summary>Flip-card countdown "<c>MM:SS</c>"; clamps to "00:00" at expiry (FR-31).</summary>
    public string RemainingText => $"{RemainingMinutes}:{RemainingSeconds}";

    private DateTimeOffset? _endsAtUtc;
    private bool _expiredRaised;

    public CountdownTimer(TimeSpan? duration = null)
    {
        var value = duration ?? TimeSpan.FromMinutes(5);
        ValidateDuration(value);
        Duration = value;
        Remaining = value;
    }

    /// <summary>Last accepted countdown span; only changes while Idle (FR-30).</summary>
    public TimeSpan Duration { get; private set; }

    /// <summary>Raised exactly once when the countdown reaches zero (FR-32).</summary>
    public event EventHandler? Expired;

    public void Start()
    {
        if (State is TimerState.Running or TimerState.Expired)
        {
            return;
        }

        State = TimerState.Running;
        _endsAtUtc = null;
    }

    public void Pause()
    {
        if (State != TimerState.Running)
        {
            return;
        }

        State = TimerState.Paused;
        _endsAtUtc = null;
    }

    public void Reset()
    {
        if (State == TimerState.Idle)
        {
            return;
        }

        State = TimerState.Idle;
        Remaining = Duration;
        _endsAtUtc = null;
        _expiredRaised = false;
    }

    /// <summary>
    /// Applies a new duration while Idle; returns false otherwise. Throws when
    /// the requested span is outside the usable MM:SS range.
    /// </summary>
    public bool SetDuration(TimeSpan duration)
    {
        ValidateDuration(duration);
        if (State != TimerState.Idle)
        {
            return false;
        }

        Duration = duration;
        Remaining = duration;
        return true;
    }

    /// <summary>
    /// Advances the countdown using the absolute instant <paramref name="now"/>
    /// from the shared tick. No-op unless <see cref="TimerState.Running"/>; an
    /// expired timer stays frozen until <see cref="Reset"/>.
    /// </summary>
    public void Evaluate(DateTimeOffset now)
    {
        if (State != TimerState.Running)
        {
            return;
        }

        _endsAtUtc ??= now + Remaining;
        var remaining = _endsAtUtc.Value - now;
        if (remaining > TimeSpan.Zero)
        {
            Remaining = remaining;
            return;
        }

        Remaining = TimeSpan.Zero;
        State = TimerState.Expired;
        _endsAtUtc = null;

        if (_expiredRaised)
        {
            return;
        }

        _expiredRaised = true;
        Expired?.Invoke(this, EventArgs.Empty);
    }

    private static void ValidateDuration(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero || duration.TotalSeconds > MaxSeconds)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), $"Duration must be in 1s..{MaxSeconds}s.");
        }
    }
}