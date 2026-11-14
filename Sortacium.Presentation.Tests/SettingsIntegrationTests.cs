using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Sortacium.Presentation.Controls;
using Sortacium.Presentation.Services;
using Sortacium.Presentation.Views;
using Xunit;

namespace Sortacium.Presentation.Tests;

public sealed class SettingsIntegrationTests
{
    // All UI checks share one dispatcher thread. No real audio device or user settings are written.
    [Fact]
    public void Settings_update_existing_controls_and_survive_restart()
    {
        var context = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(null);
        AppBuilder.Configure<App>().UseHeadless(new AvaloniaHeadlessPlatformOptions()).SetupWithoutStarting();
        var directory = Path.Combine(Path.GetTempPath(), "Sortacium.Tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "settings.json");
        var settings = new SettingsService(path);
        settings.Initialize();
        var view = new SettingsView { DataContext = settings };
        var icon = new IconButton { Text = "Test", Icon = StreamGeometry.Parse("M0,0 L10,0 L5,10 Z") };
        var window = new Window { Content = new StackPanel { Children = { view, icon } } };
        try
        {
            window.Show();
            window.UpdateLayout();
            var algorithms = new AlgorithmCodeService().GetAlgorithms();
            Assert.Equal(4, algorithms.Count);
            int localizedNotifications = 0;
            algorithms[0].PropertyChanged += (_, _) => localizedNotifications++;
            foreach (var language in settings.Languages)
            {
                settings.SelectedLanguage = language;
                window.UpdateLayout();
                Assert.Equal(language.Id, LocalizationService.Instance.Language);
                Assert.Contains(view.GetVisualDescendants().OfType<TextBlock>(), t => t.Text == LocalizationService.Instance["Settings"]);
                Assert.All(algorithms, a =>
                {
                    Assert.True(a.Definition.Translations.ContainsKey(language.Id));
                    Assert.False(string.IsNullOrWhiteSpace(a.Description));
                });
            }
            Assert.True(localizedNotifications > 0);
            Assert.Equal("Tri à bulles", algorithms[0].Name);

            var picker = view.FindControl<ListBox>("AccentPicker")!;
            Assert.Equal(8, picker.Items.Count);
            picker.SelectedValue = "Teal";
            Assert.Equal("Teal", settings.AccentId);
            Assert.Equal(Color.Parse("#37C6AA"), AppearanceService.Instance.Accent);
            Assert.Equal("Teal", AppearanceService.ResolveAccentId("#20B59B"));

            icon.Transitions = null;
            var border = Assert.Single(icon.GetVisualDescendants().OfType<BeveledBorder>());
            border.Transitions = null;
            IGradientBrush Gradient() => Assert.IsAssignableFrom<IGradientBrush>(border.BorderBrush);
            Assert.All(Gradient().GradientStops, stop => Assert.Equal((byte)255, stop.Color.A));
            Assert.Equal(Color.Parse("#353535"), Gradient().GradientStops[0].Color);
            ((IPseudoClasses)icon.Classes).Set(":pressed", true);
            Assert.Equal(Color.Parse("#191919"), Gradient().GradientStops[0].Color);
            ((IPseudoClasses)icon.Classes).Set(":pressed", false);
            Assert.Equal(Color.Parse("#353535"), Gradient().GradientStops[0].Color);

            view.FindControl<RadioButton>("LightCard")!.IsChecked = true;
            Assert.False(settings.IsDark);
            Assert.True(Gradient().GradientStops[0].Color.R > 150);
            Assert.Equal(ThemeVariant.Light, window.ActualThemeVariant);
            Assert.Equal(Color.Parse("#087260"), AppearanceService.Instance.Accent);
            Assert.Equal("Teal", picker.SelectedValue);
            view.FindControl<ToggleSwitch>("SoundToggle")!.IsChecked = false;
            Assert.False(settings.SoundEnabled);
            using (var sound = new SoundService(settings))
            {
                sound.Play(UiSound.Pressed);
                Assert.False(sound.Enabled);
                Assert.Null(typeof(SoundService).GetField("_device", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(sound));
            }
            var restored = new SettingsService(path);
            restored.Initialize();
            Assert.False(restored.IsDark);
            Assert.False(restored.SoundEnabled);
            Assert.Equal("fr", restored.SelectedLanguage.Id);
            Assert.Equal(settings.AccentId, restored.AccentId);

            File.WriteAllText(path, "invalid json");
            var fallback = new SettingsService(path);
            fallback.Initialize();
            Assert.True(fallback.IsDark);
            Assert.Equal("en", fallback.SelectedLanguage.Id);
        }
        finally
        {
            window.Close();
            if (File.Exists(path)) File.Delete(path);
            if (File.Exists(path + ".tmp")) File.Delete(path + ".tmp");
            if (Directory.Exists(directory)) Directory.Delete(directory);
            SynchronizationContext.SetSynchronizationContext(context);
        }
    }
}
