using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia;

using Sortacium.Presentation.Services;

using System.Diagnostics;

namespace Sortacium.Presentation.Controls;

[PseudoClasses(":has-text")]
public sealed class IconButton : Button
{
    private Stopwatch _stopwatch = Stopwatch.StartNew();

    public static readonly StyledProperty<StreamGeometry> IconProperty =
        AvaloniaProperty.Register<IconButton, StreamGeometry>(nameof(Icon));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<IconButton, string?>(nameof(Text));

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public StreamGeometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TextProperty)
        {
            PseudoClasses.Set(":has-text", !string.IsNullOrWhiteSpace(Text));
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        _stopwatch.Restart();

        SoundService.Instance.Play(UiSound.Pressed);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_stopwatch.ElapsedMilliseconds < 150)
        {
            return;
        }

        SoundService.Instance.Play(UiSound.Pressed);
    }
}
