using System;

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

    private void Minimize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void Maximize_Click(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OpenDialog(Border dialog)
    {
        SettingsDialog.IsVisible =
            ReferenceEquals(dialog, SettingsDialog);

        AlgorithmSelectionDialog.IsVisible =
            ReferenceEquals(dialog, AlgorithmSelectionDialog);

        ModalOverlay.IsHitTestVisible = true;
        ModalScrim.Classes.Add("open");
    }

    private void CloseDialog()
    {
        SettingsDialog.IsVisible = false;
        AlgorithmSelectionDialog.IsVisible = false;

        ModalScrim.Classes.Remove("open");
        ModalOverlay.IsHitTestVisible = false;
    }

    private void OpenSettings_Click(
        object? sender,
        RoutedEventArgs e)
    {
        OpenDialog(SettingsDialog);
    }

    private void OpenAlgorithmSelection_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        vm.BeginAlgorithmSelection();

        OpenDialog(AlgorithmSelectionDialog);
    }

    private void SettingsView_CloseRequested(
        object? sender,
        EventArgs e)
    {
        CloseDialog();
    }

    private void AlgorithmSelectionView_CloseRequested(
        object? sender,
        EventArgs e)
    {
        CloseDialog();
    }

    private void AlgorithmSelectionView_ApplyRequested(
        object? sender,
        EventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        if (vm.SwitchAlgorithm())
            CloseDialog();
    }
}
