using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Threading;

using AvaloniaEdit.Document;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Sortacium.Core.Algorithms;
using Sortacium.Core.Execution;
using Sortacium.Presentation.Models;
using Sortacium.Presentation.Services;

namespace Sortacium.Presentation.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public LibraryViewModel Library { get; }

    private static readonly CodeLanguageOption[] AllLanguages =
    [
        new("pseudocode", "Pseudocode"),
        new("c", "C"),
        new("csharp", "C#"),
        new("cpp", "C++"),
        new("python", "Python"),
        new("go", "Go"),
        new("rust", "Rust"),
        new("java", "Java"),
        new("javascript", "JavaScript"),
        new("assembly", "Assembly")
    ];

    public IReadOnlyList<AlgorithmOption> AvailableAlgorithms
    { get; }

    [ObservableProperty]
    private AlgorithmOption _selectedAlgorithm = null!;

    [ObservableProperty]
    private CodeLanguageOption? _selectedLanguage =
    new("csharp", "C#");

    [ObservableProperty]
    private IReadOnlyList<CodeLanguageOption> _availableLanguages = [];

    partial void OnSelectedAlgorithmChanged(AlgorithmOption value)
    {
        // Получаем языки, поддерживаемые новым алгоритмом.
        IReadOnlyList<CodeLanguageOption> languages =
            GetLanguageOptions(value.Id);

        string? preferredLanguage = SelectedLanguage?.Id;

        AvailableLanguages = languages;

        // Если предыдущий язык недоступен,
        // выбираем первый поддерживаемый.
        SelectedLanguage = languages.FirstOrDefault(
            language => language.Id == preferredLanguage)
            ?? languages.FirstOrDefault();

        LoadSelectedLanguageCode();

        // Только изменение активного алгоритма
        // запускает новую сортировку.
        ResetBars();
    }

    private IReadOnlyList<CodeLanguageOption> GetLanguageOptions(
    string algorithmId)
    {
        IReadOnlyCollection<string> supported =
            _codeService.GetLanguages(algorithmId);

        return AllLanguages
            .Where(language => supported.Contains(language.Id))
            .ToArray();
    }

    public void BeginAlgorithmSelection()
    {
        Library.BeginSelection(
            SelectedAlgorithm,
            SelectedLanguage);
    }

    public bool SwitchAlgorithm()
    {
        if (Library.GetSelection() is not { } selection)
            return false;

        // Только MainViewModel меняет активный алгоритм.
        // Существующий OnSelectedAlgorithmChanged вызовет ResetBars().
        SelectedAlgorithm = selection.Algorithm;

        SelectedLanguage = AvailableLanguages.FirstOrDefault(
            language => language.Id == selection.Language.Id);

        LoadSelectedLanguageCode();

        return true;
    }

    private void LoadSelectedLanguageCode()
    {
        if (SelectedAlgorithm is not { } algorithm ||
            SelectedLanguage is not { } language)
        {
            return;
        }

        if (!_codeService.GetLanguages(algorithm.Id)
            .Contains(language.Id))
        {
            return;
        }

        CodeImplementation code = _codeService.GetCode(
            algorithm.Id,
            language.Id);

        CodeDocument = new TextDocument(code.Source);

        UpdateCodeHighlight(_player?.CurrentAction?.CodePoint);
    }

    partial void OnSelectedLanguageChanged(CodeLanguageOption? value)
    {
        if (value is not null)
            LoadSelectedLanguageCode();
    }

    private SortingPlayer? _player;

    private CancellationTokenSource? _bakeCts;
    private CancellationTokenSource? _playCts;

    private int _generation;
    private int _count = 20;

    private Bar[] _barsById = [];

    private IReadOnlyList<Bar> _bars = Array.Empty<Bar>();

    public IReadOnlyList<Bar> Bars
    {
        get => _bars;
        private set => SetProperty(ref _bars, value);
    }

    public int Count
    {
        get => _count;

        set
        {
            int count = Math.Clamp(value, 5, 5000);

            if (SetProperty(ref _count, count))
                ResetBars();
        }
    }

    private readonly AlgorithmCodeService _codeService = new();

    [ObservableProperty]
    private TextDocument _codeDocument = new();

    [ObservableProperty]
    private int[] _highlightedCodeLines = [];

    private double _speed = 5;

    public double Speed
    {
        get => _speed;
        set => SetProperty(
            ref _speed,
            Math.Clamp(value, 1, 100));
    }

    [ObservableProperty]
    private bool _isBaking;

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    private int _currentStep;

    [ObservableProperty]
    private int _totalSteps;

    [ObservableProperty]
    private int _comparisonCount;

    [ObservableProperty]
    private int _swapCount;

    [ObservableProperty]
    private double _timelinePosition;

    [ObservableProperty]
    private int _snapVersion;

    [ObservableProperty]
    private string _activeCodePoint = "";

    [ObservableProperty]
    private string _variablesDescription = "";

    [ObservableProperty]
    private string _bakeError = "";

    public bool IsReady =>
        !IsBaking && _player is not null;

    public MainViewModel()
    {
        AvailableAlgorithms = _codeService.GetAlgorithms();

        Library = new LibraryViewModel(
            _codeService,
            AvailableAlgorithms,
            AllLanguages);

        // Устанавливаем начальный алгоритм без повторной
        // генерации последовательности операций.
        _selectedAlgorithm = AvailableAlgorithms.First(
            algorithm => algorithm.Id == "bubble-sort");

        AvailableLanguages = GetLanguageOptions(SelectedAlgorithm.Id);

        _selectedLanguage = AvailableLanguages.First(
            language => language.Id == "csharp");

        LoadSelectedLanguageCode();

        ResetBars();
    }

    [RelayCommand]
    private void Shuffle()
    {
        ResetBars();
    }

    private void ResetBars()
    {
        StopPlayback();

        _bakeCts?.Cancel();

        int generation = ++_generation;

        _player = null;

        double[] values = Enumerable
            .Range(1, Count)
            .Select(i => (double) i / Count)
            .ToArray();

        for (int i = values.Length - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);

            (values[i], values[j]) =
                (values[j], values[i]);
        }

        _barsById = Enumerable
            .Range(0, values.Length)
            .Select(i => new Bar((ushort)i, (float)values[i]))
            .ToArray();

        Bars = _barsById;

        CurrentStep = 0;
        TotalSteps = 0;
        ComparisonCount = 0;
        SwapCount = 0;

        TimelinePosition = 0;
        ActiveCodePoint = "";
        VariablesDescription = "";
        BakeError = "";

        IsBaking = true;

        OnPropertyChanged(nameof(IsReady));

        var cts = new CancellationTokenSource();

        _bakeCts = cts;

        ISortingAlgorithm algorithm =
    SortingAlgorithmFactory.Create(SelectedAlgorithm.Id);

        _ = BakeAsync(
            generation,
            values,
            cts,
            algorithm);

        UpdateCodeHighlight(null);
    }

    private async Task BakeAsync(
    int generation,
    double[] values,
    CancellationTokenSource cts,
    ISortingAlgorithm algorithm)
    {
        SortingTrace? trace = null;
        string? error = null;

        try
        {
            trace = await Task.Run(
                () => algorithm.Bake(values, cts.Token),
                cts.Token
            ).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Отменённое запекание.
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (!ReferenceEquals(_bakeCts, cts))
                return;

            _bakeCts = null;

            if (generation == _generation &&
                !cts.IsCancellationRequested)
            {
                if (trace is not null)
                {
                    _player = new SortingPlayer(trace);

                    TotalSteps = _player.TotalSteps;
                }
                else if (error is not null)
                {
                    BakeError = error;
                }
            }

            IsBaking = false;

            OnPropertyChanged(nameof(IsReady));
        });

        cts.Dispose();
    }

    [RelayCommand]
    private async Task PlayAsync()
    {
        if (!IsReady || _playCts is not null)
            return;

        var player = _player!;
        var cts = new CancellationTokenSource();

        _playCts = cts;
        IsPlaying = true;

        var stopwatch = Stopwatch.StartNew();

        TimeSpan previousTime = stopwatch.Elapsed;

        // Первую операцию выполняем сразу.
        double accumulatedOperations = 1;

        try
        {
            while (!cts.IsCancellationRequested &&
                   ReferenceEquals(_player, player) &&
                   player.Position < player.TotalSteps)
            {
                TimeSpan currentTime = stopwatch.Elapsed;

                // Ограничиваем скачок времени после зависания UI.
                double deltaTime = Math.Min(
                    (currentTime - previousTime).TotalSeconds,
                    0.25);

                previousTime = currentTime;

                // Speed читается заново на каждом обновлении.
                accumulatedOperations += deltaTime * Speed;

                int stepsToExecute = Math.Min(
                    (int) Math.Floor(accumulatedOperations),
                    64);

                int executed = 0;

                while (executed < stepsToExecute &&
                       player.Next())
                {
                    executed++;
                }

                accumulatedOperations -= executed;

                if (executed > 0)
                    PublishPlayerState(instant: false);

                if (player.Position >= player.TotalSteps)
                    break;

                await Task.Delay(16, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Воспроизведение остановлено.
        }
        finally
        {
            stopwatch.Stop();

            if (ReferenceEquals(_playCts, cts))
            {
                _playCts = null;
                IsPlaying = false;
            }

            cts.Dispose();
        }
    }

    [RelayCommand]
    private void Pause()
    {
        StopPlayback();
    }

    private void StopPlayback()
    {
        var cts = _playCts;

        if (cts is null)
            return;

        _playCts = null;
        IsPlaying = false;

        cts.Cancel();
    }

    [RelayCommand]
    private void NextStep()
    {
        StopPlayback();

        if (!IsReady)
            return;

        _player!.Next();

        PublishPlayerState(instant: false);
    }

    [RelayCommand]
    private void PreviousStep()
    {
        StopPlayback();

        if (!IsReady)
            return;

        _player!.Previous();

        PublishPlayerState(instant: false);
    }

    [RelayCommand]
    private void ForceToEnd()
    {
        StopPlayback();

        if (!IsReady)
            return;

        _player!.Seek(_player.TotalSteps);

        PublishPlayerState(instant: true);
    }

    partial void OnTimelinePositionChanged(double value)
    {
        if (!IsReady)
            return;

        int target = Math.Clamp(
            (int) Math.Round(value),
            0,
            _player!.TotalSteps);

        if (target == _player.Position)
            return;

        StopPlayback();

        _player.Seek(target);

        PublishPlayerState(instant: true);
    }

    private void UpdateCodeHighlight(SortingCodePoint? codePoint)
    {
        if (SelectedAlgorithm is not { } algorithm ||
            SelectedLanguage is not { } language ||
            !_codeService.GetLanguages(algorithm.Id).Contains(language.Id))
        {
            HighlightedCodeLines = [];
            return;
        }

        string? mappingKey = codePoint switch
        {
            SortingCodePoint.CompareAdjacent => "compare",
            SortingCodePoint.SwapAdjacent => "swap",
            _ => null
        };

        HighlightedCodeLines = mappingKey is null
            ? []
            : _codeService.GetHighlightedLines(
                algorithm.Id,
                language.Id,
                mappingKey);
    }

    private void PublishPlayerState(bool instant)
    {
        if (_player is not { } player)
            return;

        for (int position = 0; position < player.Count; position++)
        {
            int elementId = player.ElementAt(position);

            _barsById[elementId].Index = position;
        }

        foreach (Bar bar in _barsById)
            bar.State = BarState.Idle;

        var action = player.CurrentAction;

        if (player.Position > 0 &&
            player.Position < player.TotalSteps &&
            action is { } current)
        {
            int leftId = player.ElementAt(current.Left);
            int rightId = player.ElementAt(current.Right);

            BarState state = current.Kind switch
            {
                SortingActionKind.Compare =>
                    BarState.Comparing,

                SortingActionKind.Swap =>
                    BarState.Swapping,

                _ => BarState.Idle
            };

            _barsById[leftId].State = state;
            _barsById[rightId].State = state;
        }

        CurrentStep = player.Position;
        TotalSteps = player.TotalSteps;

        ComparisonCount = player.Comparisons;
        SwapCount = player.Swaps;

        if (action is { } step)
        {
            ActiveCodePoint = step.CodePoint.ToString();

            VariablesDescription =
                $"i = {step.I}, j = {step.J}\n" +
                $"A[{step.Left}] = {step.LeftValue:F3}\n" +
                $"A[{step.Right}] = {step.RightValue:F3}";
        }
        else
        {
            ActiveCodePoint = "";
            VariablesDescription = "";
        }

        TimelinePosition = player.Position;

        if (instant)
        {
            SnapVersion++;
        }

        UpdateCodeHighlight(action?.CodePoint);
    }
}
