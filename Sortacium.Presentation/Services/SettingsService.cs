using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Linq;
using System.IO;
using System;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Sortacium.Presentation.Services;

public sealed record InterfaceLanguage(string Id, string Name);

public sealed partial class SettingsService : ObservableObject
{
    public static SettingsService Instance { get; } = new();
    public IReadOnlyList<InterfaceLanguage> Languages { get; } = new[]
    {
        new InterfaceLanguage("ru", "Русский"),
        new("uk", "Українська"),
        new("de", "Deutsch"),
        new("en", "English"),
        new("fr", "Français")
    };

    private readonly string _path;

    private bool _initialized;

    [ObservableProperty]
    private bool _isDark = true;

    [ObservableProperty]
    private string _accentId = AppearanceService.DefaultAccent;

    [ObservableProperty]
    private InterfaceLanguage _selectedLanguage = new("en", "English");

    [ObservableProperty]
    private bool _soundEnabled = true;

    public bool IsLight
    {
        get => !IsDark;
        set
        {
            if (value) IsDark = false;
        }
    }

    public SettingsService(string? path = null)
    {
        _path =
            path
            ??
            Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
                "Sortacium",
                "settings.json");
    }

    public void Initialize()
    {
        if (_initialized)
        {
            return;
        } 
        try
        {
            if (File.Exists(_path) && JsonSerializer.Deserialize<SavedSettings>(File.ReadAllText(_path)) is { } saved)
            {
                IsDark = saved.Theme != "light";
                AccentId = AppearanceService.ResolveAccentId(saved.Accent);
                SelectedLanguage = Languages.FirstOrDefault(l => l.Id == saved.Language) ?? Languages[3];
                SoundEnabled = saved.SoundEnabled;
            }
        }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException)
        {
            Debug.WriteLine($"Settings could not be loaded: {e.Message}");
        }

        _initialized = true;

        AppearanceService.Instance.Apply(IsDark, AccentId);
        LocalizationService.Instance.SetLanguage(SelectedLanguage.Id);
    }

    partial void OnIsDarkChanged(bool value)
    {
        OnPropertyChanged(nameof(IsLight));

        ApplyAppearance();
    }

    partial void OnAccentIdChanged(string value)
    {
        ApplyAppearance();
    }

    partial void OnSelectedLanguageChanged(InterfaceLanguage value)
    {
        if (!_initialized || value is null)
        {
            return;
        }

        LocalizationService.Instance.SetLanguage(value.Id);

        Save();
    }

    partial void OnSoundEnabledChanged(bool value)
    {
        if (_initialized)
        {
            Save();
        }
    }

    private void ApplyAppearance()
    {
        if (!_initialized)
        {
            return;
        }

        AppearanceService.Instance.Apply(IsDark, AccentId);

        Save();
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

            var temporary = _path + ".tmp";

            File.WriteAllText(temporary,
                JsonSerializer.Serialize(new SavedSettings(IsDark ? "dark" : "light", AccentId, SelectedLanguage.Id, SoundEnabled),
                new JsonSerializerOptions { WriteIndented = true }));

            File.Move(temporary, _path, true);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Debug.WriteLine($"Settings could not be saved: {e.Message}");
        }
    }

    public sealed record SavedSettings(string Theme, string Accent, string Language, bool SoundEnabled);
}
