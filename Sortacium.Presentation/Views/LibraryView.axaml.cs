using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Sortacium.Presentation.Views;

public partial class LibraryView : UserControl
{
    public event EventHandler? CloseRequested;
    public event EventHandler? ApplyRequested;

    public LibraryView()
    {
        InitializeComponent();
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void Apply_Click(object? sender, RoutedEventArgs e)
    {
        ApplyRequested?.Invoke(this, EventArgs.Empty);
    }
}
