using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Sortacium.Presentation.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Minimize_Click(
       object? sender,
       RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(
        object? sender,
        RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void Close_Click(
        object? sender,
        RoutedEventArgs e)
    {
        Close();
    }

    private void OpenSettings_Click(
     object? sender,
     RoutedEventArgs e)
    {
        ModalOverlay.IsHitTestVisible = true;

        ModalOverlay.Classes.Add("open");
        SettingsDialog.Classes.Add("open");
    }

    private void CloseSettings_Click(
        object? sender,
        RoutedEventArgs e)
    {
        ModalOverlay.Classes.Remove("open");
        SettingsDialog.Classes.Remove("open");

        ModalOverlay.IsHitTestVisible = false;
    }
}
