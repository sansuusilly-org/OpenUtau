using System.Diagnostics;
using System.Runtime.InteropServices;
using AVFoundation;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using OpenUtau.Audio;

namespace OpenUtau.iOS;

/// <summary>
/// Native iOS output for short previews. Rendering remains in OpenUtau.Core;
/// this class only hands the resulting floating-point samples to AVAudioEngine.
/// </summary>
internal sealed class IosAudioOutput : IAudioOutput, IDisposable {
    const int SampleRate = 44100;
    const int Channels = 2;
    const int FramesPerBuffer = 4096;

    readonly AVAudioEngine engine = new();
    readonly AVAudioPlayerNode player = new();
    readonly AVAudioFormat format = new(SampleRate, Channels);
    readonly Stopwatch clock = new();
    readonly List<AVAudioPCMBuffer> buffers = new();
    long pausedBytes;

    public IosAudioOutput() {
        engine.AttachNode(player);
        engine.Connect(player, engine.MainMixerNode, format);
    }

    public PlaybackState PlaybackState { get; private set; } = PlaybackState.Stopped;
    public int DeviceNumber => 0;

    public List<AudioOutputDevice> GetOutputDevices() => new() {
        new AudioOutputDevice { name = "iPad audio", api = "AVAudioEngine", deviceNumber = 0, guid = Guid.Empty },
    };

    public void SelectDevice(Guid guid, int deviceNumber) { }

    public void Init(ISampleProvider sampleProvider) {
        Stop();
        buffers.Clear();
        var source = sampleProvider.WaveFormat.SampleRate == SampleRate && sampleProvider.WaveFormat.Channels == Channels
            ? sampleProvider
            : sampleProvider.ToStereo().Resample(SampleRate);
        var samples = new float[FramesPerBuffer * Channels];
        int read;
        while ((read = source.Read(samples, 0, samples.Length)) > 0) {
            var frames = (uint)((read + Channels - 1) / Channels);
            var buffer = new AVAudioPCMBuffer(format, frames);
            buffer.FrameLength = frames;
            for (var channel = 0; channel < Channels; channel++) {
                var planar = new float[frames];
                for (var frame = 0; frame < frames; frame++) {
                    var sampleIndex = frame * Channels + channel;
                    planar[frame] = sampleIndex < read ? samples[sampleIndex] : 0;
                }
                Marshal.Copy(planar, 0, buffer.FloatChannelData[channel], planar.Length);
            }
            buffers.Add(buffer);
        }
    }

    public void Play() {
        if (PlaybackState == PlaybackState.Playing) return;
        if (PlaybackState == PlaybackState.Stopped) {
            foreach (var buffer in buffers) player.ScheduleBuffer(buffer, null);
            pausedBytes = 0;
        }
        if (!engine.Running && !engine.StartAndReturnError(out var error)) {
            throw new InvalidOperationException(error?.LocalizedDescription ?? "Unable to start iPad audio output.");
        }
        player.Play();
        clock.Restart();
        PlaybackState = PlaybackState.Playing;
    }

    public void Pause() {
        if (PlaybackState != PlaybackState.Playing) return;
        pausedBytes = GetPosition();
        clock.Stop();
        player.Pause();
        PlaybackState = PlaybackState.Paused;
    }

    public void Stop() {
        clock.Reset();
        pausedBytes = 0;
        player.Stop();
        engine.Stop();
        PlaybackState = PlaybackState.Stopped;
    }

    public long GetPosition() {
        var elapsed = PlaybackState == PlaybackState.Playing ? clock.Elapsed.TotalSeconds : 0;
        return pausedBytes + (long)(elapsed * SampleRate * Channels * sizeof(float));
    }

    public void Dispose() {
        Stop();
        engine.Dispose();
        player.Dispose();
        format.Dispose();
    }
}
