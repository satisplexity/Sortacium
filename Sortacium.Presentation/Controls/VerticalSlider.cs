using Avalonia.Controls;
using Avalonia;

namespace Sortacium.Presentation.Controls;

public sealed class VerticalSlider : Slider
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<VerticalSlider, string>(nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
}
