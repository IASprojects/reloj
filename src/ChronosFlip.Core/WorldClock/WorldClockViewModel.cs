using System.Collections.ObjectModel;
using ChronosFlip.Core.Clocks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChronosFlip.Core.WorldClock;

/// <summary>
/// Owns the world-clock card tray: the always-present local card plus zero or
/// more user zones. Reacts to a single shared <see cref="ClockTicker"/> so one
/// 1s pump advances every card from the same instant (FR-11, NFR-02). The local
/// card can never be removed.
/// </summary>
public partial class WorldClockViewModel : ObservableObject
{
    private readonly IZoneResolver _resolver;
    private readonly WorldClockCardViewModel _localCard;
    private ClockTicker? _ticker;

    public WorldClockViewModel(IZoneResolver resolver, IEnumerable<ClockZone> zones)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

        var localZone = _resolver.Local
            ?? throw new ArgumentException("Resolver must expose a valid local zone.", nameof(resolver));

        _localCard = new WorldClockCardViewModel(
            ClockZoneFactory.DefaultLabel(localZone), localZone.Id, localZone)
        {
            IsRemovable = false,
        };
        Cards.Add(_localCard);

        foreach (var zone in zones)
        {
            AddZone(zone);
        }
    }

    public ObservableCollection<WorldClockCardViewModel> Cards { get; } = new();

    public WorldClockCardViewModel LocalCard => _localCard;

    /// <summary>Adds a user zone card; false if unresolved, local, or a duplicate.</summary>
    public bool AddZone(ClockZone zone)
    {
        ArgumentNullException.ThrowIfNull(zone);

        var resolved = _resolver.Resolve(zone.TimeZoneId);
        if (resolved is null ||
            Cards.Any(card => string.Equals(card.TimeZoneId, zone.TimeZoneId, StringComparison.Ordinal)))
        {
            return false;
        }

        var label = string.IsNullOrWhiteSpace(zone.Label)
            ? ClockZoneFactory.DefaultLabel(resolved)
            : zone.Label;

        Cards.Add(new WorldClockCardViewModel(label, zone.TimeZoneId, resolved));
        return true;
    }

    /// <summary>Removes a user zone card by id; false if missing or the local card.</summary>
    public bool RemoveZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return false;
        }

        var card = Cards.FirstOrDefault(
            candidate => string.Equals(candidate.TimeZoneId, timeZoneId, StringComparison.Ordinal));
        if (card is null || ReferenceEquals(card, _localCard))
        {
            return false;
        }

        return Cards.Remove(card);
    }

    /// <summary>Non-local zones in tray order, ready to persist (FR-12).</summary>
    public IReadOnlyList<ClockZone> ZonesToPersist() => Cards
        .Where(card => !ReferenceEquals(card, _localCard))
        .Select(card => new ClockZone { Label = card.Label, TimeZoneId = card.TimeZoneId })
        .ToList();

    /// <summary>Shares a tick source; re-attaching the same ticker is a no-op.</summary>
    public void Attach(ClockTicker ticker)
    {
        ArgumentNullException.ThrowIfNull(ticker);
        if (ReferenceEquals(_ticker, ticker))
        {
            return;
        }

        if (_ticker is not null)
        {
            _ticker.Tick -= OnTick;
        }

        _ticker = ticker;
        ticker.Tick += OnTick;
        ApplyInstant(ticker.Now);
    }

    private void OnTick(object? sender, DateTimeOffset now) => ApplyInstant(now);

    private void ApplyInstant(DateTimeOffset instant)
    {
        foreach (var card in Cards)
        {
            card.SetPresent(instant);
        }
    }
}