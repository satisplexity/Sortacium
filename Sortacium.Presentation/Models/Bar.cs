using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sortacium.Presentation.Models;

public enum BarState
{
    Initialize,
    Idle,
    Comparing,
    Swapping
}

public class Bar : INotifyPropertyChanged
{
    private int _index;
    private double _value;
    private BarState _state = BarState.Initialize;

    public Bar(int index, double value)
    {
        _index = index;
        _value = Math.Clamp(value, 0, 1);
    }

    public int Index
    {
        get => _index;
        set => Set(ref _index, value);
    }

    public double Value
    {
        get => _value;
        set => Set(ref _value, Math.Clamp(value, 0, 1));
    }

    public BarState State
    {
        get => _state;
        set => Set(ref _state, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void Set<T>(
        ref T field,
        T value,
        [CallerMemberName] string? property = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;

        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(property));
    }
}
