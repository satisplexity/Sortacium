using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sortacium.Presentation.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sortacium.Presentation.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Welcome to Avalonia!";

    [ObservableProperty]
    public IReadOnlyList<Bar> Bars { get; } = new List<Bar>
    {
        new Bar(0, 0.2),
        new Bar(1, 0.5),
        new Bar(2, 0.3),
        new Bar(3, 0.8),
        new Bar(4, 0.1),
        new Bar(5, 0.6),
        new Bar(6, 0.4),
        new Bar(7, 0.9),
        new Bar(8, 0.7),
        new Bar(9, 1.0)
    };

    [RelayCommand]
    public async Task BubbleSortCommand()
    {
        await BubbleSortAsync();
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
