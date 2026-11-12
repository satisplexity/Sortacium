using System;

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Sortacium.Presentation.Views;

public partial class SettingsView : UserControl
{
    public event EventHandler? CloseRequested;

    public SettingsView()
    {
        InitializeComponent();
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}
