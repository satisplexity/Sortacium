using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Providers;
using SoundFlow.Structs;

using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System;

namespace Sortacium.Presentation.Services;

public enum UiSound
{
    Pressed,
    Released,
    Tick
}

public sealed class SoundService : IDisposable
{
    public static SoundService Instance { get; } = new();

    private readonly MiniAudioEngine _engine;
    private readonly AudioPlaybackDevice _device;
    private readonly byte[][] _sounds;

    public bool Enabled { get; set; } = true;

    private SoundService()
    {
        _sounds =
        [
            Load("pressed.wav"),
            Load("released.wav"),
            Load("tick.wav")
        ];

        _engine = new MiniAudioEngine();
        _device = _engine.InitializePlaybackDevice(
            null, AudioFormat.Dvd);

        _device.Start();
    }

    private static byte[] Load(string filename)
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Assets", "Sounds", filename);

        return File.ReadAllBytes(path);
    }

    public void Play(UiSound sound)
    {
        if (!Enabled)
            return;

        _ = PlayAsync(sound);
    }

    private async Task PlayAsync(UiSound sound)
    {
        try
        {
            using var stream = new MemoryStream(
                _sounds[(int)sound], writable: false);

            using var provider = new StreamDataProvider(
                _engine, _device.Format, stream);

            using var player = new SoundPlayer(
                _engine, _device.Format, provider);

            _device.MasterMixer.AddComponent(player);

            try
            {
                player.Play();

                await Task.Delay((int)(player.Duration * 1000));
            }
            finally
            {
                player.Stop();
                _device.MasterMixer.RemoveComponent(player);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Sound error: {ex}");
        }
    }

    public void Dispose()
    {
        _device.Stop();
        _device.Dispose();
        _engine.Dispose();
    }
}
