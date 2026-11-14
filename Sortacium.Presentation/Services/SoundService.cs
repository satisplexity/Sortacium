using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Providers;
using SoundFlow.Structs;

using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System;

namespace Sortacium.Presentation.Services;

public enum UiSound { Pressed, Released, Tick }

public sealed class SoundService : IDisposable
{
    public static SoundService Instance { get; } = new(SettingsService.Instance);
    private readonly SettingsService _settings;
    private readonly object _sync = new();
    private readonly HashSet<SoundPlayer> _players = [];
    private MiniAudioEngine? _engine;
    private AudioPlaybackDevice? _device;
    private byte[][]? _sounds;
    private bool _disposed;

    public bool Enabled
    {
        get => _settings.SoundEnabled;
        set => _settings.SoundEnabled = value;
    }

    public SoundService(SettingsService settings)
    {
        _settings = settings;
        _settings.PropertyChanged += OnSettingsChanged;
    }

    private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SettingsService.SoundEnabled) || Enabled)
        {
            return;
        }

        lock (_sync)
        {
            foreach (var player in _players) player.Stop();
        }
    }

    private void EnsurePlayback()
    {
        if (_device is not null)
        {
            return;
        }
            
        _sounds ??= [Load("pressed.wav"), Load("released.wav"), Load("tick.wav")];

        var engine = new MiniAudioEngine();

        try
        {
            var device = engine.InitializePlaybackDevice(null, AudioFormat.Dvd);

            device.Start();

            _engine = engine;
            _device = device;
        }
        catch
        {
            engine.Dispose();
            throw;
        }
    }

    private static byte[] Load(string filename)
    {
        return File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Assets", "Sounds", filename));
    }
    public void Play(UiSound sound)
    {
        lock (_sync)
        {
            if (!Enabled || _disposed)
            {
                return;
            } 
            try
            {
                EnsurePlayback();
                _ = PlayAsync(sound);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Sound error: {ex}");
            }
        }
    }

    private async Task PlayAsync(UiSound sound)
    {
        try
        {
            using var stream = new MemoryStream(_sounds![(int)sound], writable: false);
            using var provider = new StreamDataProvider(_engine!, _device!.Format, stream);
            using var player = new SoundPlayer(_engine!, _device.Format, provider);

            _device.MasterMixer.AddComponent(player);
            _players.Add(player);

            try
            {
                player.Play();

                await Task.Delay((int)(player.Duration * 1000));
            }
            finally
            {
                lock (_sync)
                {
                    _players.Remove(player);

                    if (!_disposed)
                    {
                        player.Stop();

                        _device.MasterMixer.RemoveComponent(player);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Sound error: {ex}");
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _settings.PropertyChanged -= OnSettingsChanged;

            foreach (var player in _players)
            {
                player.Stop();
            }

            _device?.Stop();
            _device?.Dispose();
            _engine?.Dispose();
        }
    }
}
