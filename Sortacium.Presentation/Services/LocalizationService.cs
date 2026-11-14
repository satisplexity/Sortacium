using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System;

using Avalonia.Platform;
using Avalonia;

namespace Sortacium.Presentation.Services;

public sealed class LocalizationService
{
    public static LocalizationService Instance { get; } = new();

    private Dictionary<string, string> _strings = [];

    public string Language { get; private set; } = "en";

    public event EventHandler? LanguageChanged;

    public string this[string key] => _strings.GetValueOrDefault(key, key);

    public void SetLanguage(string language)
    {
        if (language is not ("ru" or "uk" or "de" or "en" or "fr"))
        {
            language = "en";
        }
            
        using var stream =
            AssetLoader.Open(new Uri($"avares://Sortacium.Presentation/Resources/Localization/{language}.json"));

        var strings =
            JsonSerializer.Deserialize<Dictionary<string, string>>(stream)
            ??
            throw new InvalidDataException("Invalid localization dictionary.");

        Language = language;
        _strings = strings;

        if (Application.Current is { } app)
        {
            foreach (var (key, value) in strings) app.Resources["L." + key] = value;
        }
            
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }
}
