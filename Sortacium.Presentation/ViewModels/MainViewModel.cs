using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Sortacium.Core.Algorithms;
using Sortacium.Core.Execution;
using Sortacium.Presentation.Models;

namespace Sortacium.Presentation.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly BubbleSortAlgorithm _algorithm = new();

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

        _ = BakeAsync(
            generation,
            values,
            cts);
    }

    private async Task BakeAsync(
        int generation,
        double[] values,
        CancellationTokenSource cts)
    {
        SortingTrace? trace = null;
        string? error = null;

        try
        {
            trace = await Task.Run(
                () => _algorithm.Bake(values, cts.Token),
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
        {
            return;
        }
            
        var player = _player!;

        var cts = new CancellationTokenSource();

        _playCts = cts;
        IsPlaying = true;

        try
        {
            while (!cts.IsCancellationRequested &&
                   ReferenceEquals(_player, player) &&
                   player.Next())
            {
                PublishPlayerState(instant: false);

                if (player.Position == player.TotalSteps)
                    break;

                await Task.Delay(300, cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // Остановка воспроизведения.
        }
        finally
        {
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
    }
}
