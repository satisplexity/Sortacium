using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;

namespace Sortacium.Presentation.Services;

public sealed class AppearanceService : INotifyPropertyChanged
{
    public static AppearanceService Instance { get; } = new();
    public bool IsDark { get; private set; } = true;
    public Color Accent { get; private set; } = Color.Parse("#AF4BE9");
    public event EventHandler? Changed;
    public event PropertyChangedEventHandler? PropertyChanged;
    public IReadOnlyDictionary<string, Color> Colors { get; private set; } = new Dictionary<string, Color>();
    private Dictionary<string, string>? _palette;

    public void Apply(bool dark, Color accent)
    {
        IsDark = dark;
        Accent = Color.FromRgb(accent.R, accent.G, accent.B);
        if (Application.Current is not { } app) return;
        if (_palette is null)
        {
            using var stream = AssetLoader.Open(new Uri("avares://Sortacium.Presentation/Resources/Palette.json"));
            _palette = JsonSerializer.Deserialize<Dictionary<string, string>>(stream)!;
        }
        app.RequestedThemeVariant = dark ? ThemeVariant.Dark : ThemeVariant.Light;
        var colors = new Dictionary<string, Color>();
        foreach (var (key, source) in _palette)
        {
            var color = Resolve(Color.Parse(source));
            colors[key] = color;
            app.Resources["Palette.Color." + key] = color;
            app.Resources["Palette.Brush." + key] = new SolidColorBrush(color);
        }
        Colors = colors;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Colors)));
        app.Resources["Color.WindowBackground"] = Resolve(Color.Parse("#191919"));
        app.Resources["Color.TitleBarBackground"] = Resolve(Color.Parse("#1F1F1F"));
        app.Resources["AccentBrush"] = new SolidColorBrush(Accent);
        // Fluent controls (including the color picker) use the same accent family.
        foreach (var key in new[] { "SystemAccentColor", "SystemAccentColorLight1", "SystemAccentColorLight2", "SystemAccentColorLight3", "SystemAccentColorDark1", "SystemAccentColorDark2", "SystemAccentColorDark3" })
            app.Resources[key] = Resolve(Color.Parse("#AF4BE9"));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public Color Resolve(Color original)
    {
        var hsv = original.ToHsv();
        // Violet shades are the existing design's accent family; neutral/lilac text stays neutral.
        if (hsv.H >= 260 && hsv.H <= 310 && hsv.S > 0.18)
        {
            var selected = Accent.ToHsv();
            var baseline = Color.Parse("#AF4BE9").ToHsv();
            double saturation = Math.Clamp(hsv.S * selected.S / baseline.S, 0, 1);
            double value = Math.Clamp(hsv.V * selected.V / baseline.V, 0, 1);
            if (!IsDark)
            {
                bool surface = hsv.V < 0.46;
                value = surface ? 0.97 : Math.Min(value, 0.70);
                saturation = surface ? saturation * 0.22 : saturation;
            }
            return new HsvColor(original.A / 255.0, selected.H, saturation, value).ToRgb();
        }
        if (IsDark || original.A < 255) return original;
        // Reverse the neutral surface/text ramp while retaining the sculpted border contrast.
        if (hsv.S < 0.19)
        {
            byte channel = (byte)Math.Clamp(255 - (original.R + original.G + original.B) / 3, 20, 248);
            return Color.FromArgb(original.A, channel, channel, channel);
        }
        return new HsvColor(original.A / 255.0, hsv.H, hsv.S, Math.Min(hsv.V, 0.62)).ToRgb();
    }
}
