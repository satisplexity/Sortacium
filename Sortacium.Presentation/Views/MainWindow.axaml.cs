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
        // Сбрасываем состояние обоих диалогов.
        SettingsDialog.Classes.Remove("open");
        AlgorithmSelectionDialog.Classes.Remove("open");

        SettingsDialog.IsVisible =
            ReferenceEquals(dialog, SettingsDialog);

        AlgorithmSelectionDialog.IsVisible =
            ReferenceEquals(dialog, AlgorithmSelectionDialog);

        ModalOverlay.IsHitTestVisible = true;
        ModalOverlay.Classes.Add("open");

        dialog.Classes.Add("open");
    }

    private void CloseDialog()
    {
        SettingsDialog.Classes.Remove("open");
        AlgorithmSelectionDialog.Classes.Remove("open");

        ModalOverlay.Classes.Remove("open");
        ModalOverlay.IsHitTestVisible = false;

        // IsVisible здесь не выключаем:
        // существующие анимации закрытия должны завершиться.
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
