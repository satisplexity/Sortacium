using Avalonia;
using Avalonia.Controls;

namespace Sortacium.Presentation.Controls;

public sealed class HorizontalSlider : Slider
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<HorizontalSlider, string>(nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
}
