using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sortacium.Presentation.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sortacium.Presentation.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private int _count = 20;

    [ObservableProperty]
    public int Count
    {
        get => _count;
        set
        {
            int count = Math.Clamp(value, 5, 500);

            if (SetProperty(ref _count, count))
                ResetBars();
        }
    }

    
    private IReadOnlyList<Bar> _bars = Array.Empty<Bar>();
    [ObservableProperty]
    public IReadOnlyList<Bar> Bars
    {
        get => _bars;
        private set => SetProperty(ref _bars, value);
    }

    [RelayCommand]
    public async Task BubbleSortCommand()
    {
        await BubbleSortAsync();
    }

    public MainViewModel()
    {
        ResetBars();
    }

    private void ResetBars()
    {
        double[] values = Enumerable.Range(1, Count)
        .Select(i => (double) i / Count)
        .ToArray();

        // Fisher-Yates shuffle.
        for (int i = values.Length - 1; i > 0; i--)
        {
            int j = Random.Shared.Next(i + 1);

            (values[i], values[j]) =
                (values[j], values[i]);
        }

        List<Bar> newBars = Enumerable.Range(0, Count)
            .Select(i => new Bar(i, values[i]))
            .ToList();

        Bars = newBars;
    }

    public async Task BubbleSortAsync(
    CancellationToken cancellationToken = default)
    {
        // Отдельный логический порядок.
        // Исходную коллекцию Bars не трогаем.
        Bar[] order = Bars
            .OrderBy(bar => bar.Index)
            .ToArray();

        for (int i = 0; i < order.Length - 1; i++)
        {
            for (int j = 0; j < order.Length - i - 1; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Bar a = order[j];
                Bar b = order[j + 1];

                a.State = BarState.Comparing;
                b.State = BarState.Comparing;

                await Task.Delay(100, cancellationToken);

                if (a.Value > b.Value)
                {
                    a.State = BarState.Swapping;
                    b.State = BarState.Swapping;

                    // Только изменение свойств моделей.
                    (a.Index, b.Index) = (b.Index, a.Index);

                    // Меняется локальная логическая последовательность,
                    // но не исходная коллекция Bars.
                    (order[j], order[j + 1]) =
                        (order[j + 1], order[j]);

                    await Task.Delay(300, cancellationToken);
                }

                a.State = BarState.Idle;
                b.State = BarState.Idle;
            }
        }
    }
}
