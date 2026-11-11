using System;
using System.Linq;

using Avalonia;
using Avalonia.Media;

using AvaloniaEdit;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;

using TextMateSharp.Grammars;


namespace Sortacium.Presentation.Controls;

public sealed class SortingCodeEditor : TextEditor
{
    protected override Type StyleKeyOverride => typeof(TextEditor);

    public static readonly StyledProperty<string> LanguageProperty =
        AvaloniaProperty.Register<SortingCodeEditor, string>(
            nameof(Language), "csharp");

    public static readonly StyledProperty<int[]> HighlightedLinesProperty =
        AvaloniaProperty.Register<SortingCodeEditor, int[]>(
            nameof(HighlightedLines), []);

    public string Language
    {
        get => GetValue(LanguageProperty);
        set => SetValue(LanguageProperty, value);
    }

    public int[] HighlightedLines
    {
        get => GetValue(HighlightedLinesProperty);
        set => SetValue(HighlightedLinesProperty, value);
    }

    private readonly RegistryOptions _registry =
        new(ThemeName.DarkPlus);

    private readonly AvaloniaEdit.TextMate.TextMate.Installation
        _installation;

    private readonly StepHighlightRenderer _renderer;

    public SortingCodeEditor()
    {
        IsReadOnly = true;
        ShowLineNumbers = true;
        WordWrap = false;

        _renderer = new StepHighlightRenderer();

        TextArea.TextView.BackgroundRenderers.Add(_renderer);

        _installation = this.InstallTextMate(_registry);

        ApplyLanguage();
    }

    protected override void OnPropertyChanged(
        AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == LanguageProperty &&
            _installation is not null)
        {
            ApplyLanguage();
        }

        if (change.Property == HighlightedLinesProperty &&
            _renderer is not null)
        {
            _renderer.SetLines(
                TextArea.TextView,
                HighlightedLines);

            int line = HighlightedLines.FirstOrDefault();

            if (line > 0 &&
                Document is not null &&
                line <= Document.LineCount)
            {
                ScrollToLine(line);
            }
        }
    }

    private void ApplyLanguage()
    {
        string? extension = Language switch
        {
            "c" => ".c",
            "csharp" => ".cs",
            "cpp" => ".cpp",
            "python" => ".py",
            "go" => ".go",
            "rust" => ".rs",
            "java" => ".java",
            "javascript" => ".js",
            "assembly" => ".asm",
            _ => null
        };

        if (extension is null)
        {
            _installation.SetGrammar(null);
            return;
        }

        var language = _registry.GetLanguageByExtension(
            extension);

        string? scope = language is null
            ? null
            : _registry.GetScopeByLanguageId(language.Id);

        _installation.SetGrammar(scope);
    }

    private sealed class StepHighlightRenderer : IBackgroundRenderer
    {
        private int[] _lines = [];

        private readonly IBrush _background =
            new SolidColorBrush(Color.Parse("#3D204D"));

        public KnownLayer Layer => KnownLayer.Background;

        public void SetLines(TextView view, int[] lines)
        {
            _lines = lines;
            view.InvalidateLayer(Layer);
        }

        public void Draw(
            TextView textView,
            DrawingContext context)
        {
            foreach (var visualLine in textView.VisualLines)
            {
                int first = visualLine.FirstDocumentLine.LineNumber;
                int last = visualLine.LastDocumentLine.LineNumber;

                if (!_lines.Any(line =>
                    line >= first && line <= last))
                {
                    continue;
                }

                double y =
                    visualLine.VisualTop - textView.ScrollOffset.Y;

                var rect = new Rect(
                    0,
                    y,
                    textView.Bounds.Width,
                    visualLine.Height);

                context.DrawRectangle(
                    _background,
                    null,
                    rect);
            }
        }
    }
}
