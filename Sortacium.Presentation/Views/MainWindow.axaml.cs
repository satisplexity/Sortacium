using Avalonia.Controls;
using Avalonia.Interactivity;

using Sortacium.Presentation.ViewModels;

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
        AlgorithmSelectionDialog.IsVisible = false;
        SettingsDialog.Classes.Add("open");
    }

    private void OpenAlgrorithmSelection_Click(
    object? sender,
    RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        // Инициализация предварительного выбора.
        vm.BeginAlgorithmSelection();

        SettingsDialog.Classes.Remove("open");
        SettingsDialog.IsVisible = false;

        AlgorithmSelectionDialog.IsVisible = true;

        ModalOverlay.IsHitTestVisible = true;
        ModalOverlay.Classes.Add("open");

        AlgorithmSelectionDialog.Classes.Add("open");
    }

    private void CloseSettings_Click(
        object? sender,
        RoutedEventArgs e)
    {
        ModalOverlay.Classes.Remove("open");
        SettingsDialog.Classes.Remove("open");
        AlgorithmSelectionDialog.IsVisible = true;
        ModalOverlay.IsHitTestVisible = false;
    }

    private void CloseAlgrorithmSelection_Click(
    object? sender,
    RoutedEventArgs e)
    {
        AlgorithmSelectionDialog.Classes.Remove("open");
        ModalOverlay.Classes.Remove("open");

        ModalOverlay.IsHitTestVisible = false;
    }

    private void SwitchAlgorithm_Click(
    object? sender,
    RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        if (!vm.SwitchAlgorithm())
            return;

        CloseAlgrorithmSelection_Click(sender, e);
    }
}
