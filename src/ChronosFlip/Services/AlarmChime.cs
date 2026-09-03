using System.Text;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace ChronosFlip.Services;

/// <summary>
/// Looping alarm chime generated in memory (PCM WAV sine tones). No audio
/// assets and no NuGet packages beyond the Windows App SDK; the OS loops the
/// stream internally, so there is no per-alarm timer (NFR-02). Safe to call
/// from the UI-thread ticks used by the alarm service (FR-22).
/// </summary>
public sealed class AlarmChime : IDisposable
{
    private MediaPlayer? _player;
    private InMemoryRandomAccessStream? _stream;

    /// <summary>Start looping. Idempotent — no-op while already ringing.</summary>
    public void Start()
    {
        if (_player is not null)
        {
            return;
        }

        var wav = BuildChimePcmWav(seconds: 0.75);

        var stream = new InMemoryRandomAccessStream();
        var writer = new DataWriter(stream);
        writer.WriteBytes(wav);
        writer.StoreAsync().AsTask().GetAwaiter().GetResult();
        writer.DetachStream();
        stream.Seek(0);

        _stream = stream;
        _player = new MediaPlayer
        {
            IsLoopingEnabled = true,
            Volume = 0.9,
        };
        _player.SetStreamSource(_stream);
        _player.Play();
    }

    /// <summary>Silence and release. Idempotent.</summary>
    public void Stop()
    {
        var player = _player;
        var stream = _stream;
        _player = null;
        _stream = null;

        if (player is not null)
        {
            player.Pause();
            player.Dispose();
        }

        stream?.Dispose();
    }

    public void Dispose() => Stop();

    private static byte[] BuildChimePcmWav(double seconds, double primaryHz = 880.0, double secondaryHz = 1174.66)
    {
        const int sampleRate = 44100;
        const short channels = 1;
        const short bitsPerSample = 16;
        int sampleCount = (int)(sampleRate * seconds);
        int dataSize = sampleCount * channels * (bitsPerSample / 8);
        int fadeSamples = sampleRate / 100;

        var wav = new byte[44 + dataSize];

        Encoding.ASCII.GetBytes("RIFF").CopyTo(wav, 0);
        BitConverter.GetBytes(36 + dataSize).CopyTo(wav, 4);
        Encoding.ASCII.GetBytes("WAVE").CopyTo(wav, 8);
        Encoding.ASCII.GetBytes("fmt ").CopyTo(wav, 12);
        BitConverter.GetBytes(16).CopyTo(wav, 16);
        BitConverter.GetBytes((short)1).CopyTo(wav, 20);
        BitConverter.GetBytes(channels).CopyTo(wav, 22);
        BitConverter.GetBytes(sampleRate).CopyTo(wav, 24);
        BitConverter.GetBytes(sampleRate * channels * bitsPerSample / 8).CopyTo(wav, 28);
        BitConverter.GetBytes((short)(channels * bitsPerSample / 8)).CopyTo(wav, 32);
        BitConverter.GetBytes(bitsPerSample).CopyTo(wav, 34);
        Encoding.ASCII.GetBytes("data").CopyTo(wav, 36);
        BitConverter.GetBytes(dataSize).CopyTo(wav, 40);

        const double volume = 0.25;
        for (int i = 0; i < sampleCount; i++)
        {
            double t = (double)i / sampleRate;
            double envelope = 1.0;
            if (i < fadeSamples)
            {
                envelope = (double)i / fadeSamples;
            }
            else if (i > sampleCount - fadeSamples)
            {
                envelope = (double)(sampleCount - i) / fadeSamples;
            }

            double tone = Math.Sin(2 * Math.PI * primaryHz * t) + Math.Sin(2 * Math.PI * secondaryHz * t);
            tone *= 0.5;
            short sample = (short)(tone * envelope * volume * short.MaxValue);
            BitConverter.GetBytes(sample).CopyTo(wav, 44 + i * 2);
        }

        return wav;
    }
}