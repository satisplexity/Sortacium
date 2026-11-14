using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;

namespace Sortacium.Presentation.Services;

public sealed class AppearanceService
{
    public const string DefaultAccent = "Violet";
    public static AppearanceService Instance { get; } = new();

    private static readonly string[] AccentIds =
        [DefaultAccent, "Blue", "Cyan", "Teal", "Green", "Amber", "Orange", "Rose"];

    private readonly Dictionary<string, ResourceDictionary> _accents = new();
    private IResourceDictionary? _applicationResources;
    private ResourceDictionary? _activeAccent;

    public bool IsDark { get; private set; } = true;
    public Color Accent => GetAccentColor("Color.Accent.Primary");
    public Color EditorHighlight => GetAccentColor("Color.Accent.Highlight");
    public event EventHandler? Changed;

    public static string ResolveAccentId(string? saved)
    {
        if (saved is not null && AccentIds.Contains(saved, StringComparer.Ordinal))
        {
            return saved;
        }
            
        if (Color.TryParse(saved, out var legacy))
        {
            var palette = Instance;

            return AccentIds
                .MinBy(id => ColorDistance(legacy, ReadColor(
                palette.LoadAccent(id), "Color.Accent.Primary", ThemeVariant.Dark)))!;
        }

        return DefaultAccent;
    }

    public void Apply(bool dark, string accentId)
    {
        IsDark = dark;

        var accent = LoadAccent(AccentIds.Contains(accentId, StringComparer.Ordinal) ? accentId : DefaultAccent);

        if (Application.Current is { } app)
        {
            var resources = app.Resources;

            if (_applicationResources != resources || _activeAccent != accent)
            {
                if (_activeAccent is not null)
                {
                    _applicationResources?.MergedDictionaries.Remove(_activeAccent);
                }

                resources.MergedDictionaries.Add(accent);
                _applicationResources = resources;
            }

            _activeAccent = accent;
            app.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        }
        else
        {
            _activeAccent = accent;
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    private ResourceDictionary LoadAccent(string id)
    {
        if (!_accents.TryGetValue(id, out var dictionary))
        {
            var uri = new Uri($"avares://Sortacium.Presentation/Themes/Palettes/Accents/{id}.axaml");
            dictionary = (ResourceDictionary)AvaloniaXamlLoader.Load(uri);
            _accents.Add(id, dictionary);
        }

        return dictionary;
    }

    private Color GetAccentColor(string key) =>
        ReadColor(_activeAccent ?? LoadAccent(DefaultAccent), key, IsDark ? ThemeVariant.Dark : ThemeVariant.Light);

    private static Color ReadColor(ResourceDictionary dictionary, string key, ThemeVariant variant) =>
        dictionary.TryGetResource(key, variant, out var value) && value is Color color
            ? color
            : throw new InvalidOperationException($"Missing color resource {key} for {variant}.");

    private static int ColorDistance(Color first, Color second)
    {
        int red = first.R - second.R;
        int green = first.G - second.G;
        int blue = first.B - second.B;
        return red * red + green * green + blue * blue;
    }
}
