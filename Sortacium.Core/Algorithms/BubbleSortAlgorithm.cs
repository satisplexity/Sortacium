using Sortacium.Core.Execution;

namespace Sortacium.Core.Algorithms;

public sealed class BubbleSortAlgorithm
{
    public SortingTrace Bake(
        IReadOnlyList<double> values,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        double[] initial = values.ToArray();
        double[] working = (double[])initial.Clone();

        var actions = new List<SortingAction>();

        ushort n = (ushort)working.Length;

        for (ushort i = 0; i < n - 1; i++)
        {
            for (ushort j = 0; j < n - i - 1; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                double left = working[j];
                double right = working[j + 1];

                actions.Add(new SortingAction(
                    SortingActionKind.Compare,
                    j,
                    (ushort)(j + 1),
                    i,
                    j,
                    left,
                    right));

                if (left <= right)
                {
                    continue;
                }
                   
                actions.Add(new SortingAction(
                    SortingActionKind.Swap,
                    j,
                    (ushort)(j + 1),
                    i,
                    j,
                    left,
                    right));

                (working[j], working[j + 1]) =
                    (working[j + 1], working[j]);
            }
        }

        return new SortingTrace(initial, actions.ToArray());
    }
}
