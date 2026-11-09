using System;
using Avalonia.Animation.Easings;

namespace Sortacium.Presentation.Components;

public class EnchancedBackEaseOut : Easing
{
    public double Amplitude { get; set; } = 1.0;

    public override double Ease(double progress)
    {
        double p = 1.0 - Math.Clamp(progress, 0, 1);

        return 1.0 - p * (
            p * p - Amplitude * Math.Sin(p * Math.PI));
    }
}
