using System.Diagnostics;
using NAudio.Dsp;
using NAudio.Wave;

namespace WebDesk;

/// <summary>Audio band levels (0..1), pushed to audio-reactive shader wallpapers.</summary>
internal readonly record struct AudioLevels(float Bass, float Mid, float Treble, float Level);

/// <summary>
/// Captures the system audio output (WASAPI loopback) and emits coarse FFT
/// band levels ~30×/second. Only runs while an audio-reactive shader is active.
/// </summary>
internal sealed class AudioCapture : IDisposable
{
    private const int FftSize = 1024; // power of two

    private WasapiLoopbackCapture? _capture;
    private readonly Complex[] _fft = new Complex[FftSize];
    private int _filled;
    private readonly Stopwatch _throttle = new();

    public event Action<AudioLevels>? LevelsAvailable;

    public bool IsRunning => _capture is not null;

    public void Start()
    {
        if (_capture is not null) return;
        try
        {
            _capture = new WasapiLoopbackCapture();
            _capture.DataAvailable += OnData;
            _filled = 0;
            _throttle.Restart();
            _capture.StartRecording();
        }
        catch
        {
            // No render device / capture unavailable: stay silent.
            Stop();
        }
    }

    public void Stop()
    {
        if (_capture is null) return;
        try
        {
            _capture.DataAvailable -= OnData;
            _capture.StopRecording();
            _capture.Dispose();
        }
        catch { /* ignore */ }
        finally { _capture = null; }
    }

    private void OnData(object? sender, WaveInEventArgs e)
    {
        var format = _capture?.WaveFormat;
        if (format is null || format.BitsPerSample != 32) return; // loopback is IEEE float

        var channels = Math.Max(1, format.Channels);
        double sumSquares = 0;
        var samples = 0;

        for (var i = 0; i + 4 * channels <= e.BytesRecorded; i += 4 * channels)
        {
            // Average channels to mono.
            float mono = 0;
            for (var c = 0; c < channels; c++)
                mono += BitConverter.ToSingle(e.Buffer, i + 4 * c);
            mono /= channels;

            sumSquares += mono * mono;
            samples++;

            if (_filled < FftSize)
            {
                // Hann window for cleaner bands.
                var w = (float)(0.5 * (1 - Math.Cos(2 * Math.PI * _filled / (FftSize - 1))));
                _fft[_filled].X = mono * w;
                _fft[_filled].Y = 0;
                _filled++;
            }
        }

        var level = samples > 0 ? (float)Math.Sqrt(sumSquares / samples) : 0f;

        if (_filled >= FftSize && _throttle.ElapsedMilliseconds >= 30)
        {
            _throttle.Restart();
            EmitBands(level);
            _filled = 0;
        }
    }

    private void EmitBands(float level)
    {
        FastFourierTransform.FFT(true, (int)Math.Log2(FftSize), _fft);

        float Band(int from, int to)
        {
            double sum = 0;
            for (var i = from; i < to && i < FftSize / 2; i++)
                sum += Math.Sqrt(_fft[i].X * _fft[i].X + _fft[i].Y * _fft[i].Y);
            var n = Math.Max(1, to - from);
            return Clamp((float)(sum / n) * 8f);
        }

        var levels = new AudioLevels(
            Bass: Band(1, 16),
            Mid: Band(16, 96),
            Treble: Band(96, 300),
            Level: Clamp(level * 4f));

        LevelsAvailable?.Invoke(levels);
    }

    private static float Clamp(float v) => v < 0 ? 0 : v > 1 ? 1 : v;

    public void Dispose() => Stop();
}
