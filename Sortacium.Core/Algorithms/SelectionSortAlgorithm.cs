using Sortacium.Core.Execution;

namespace Sortacium.Core.Algorithms;

public sealed class SelectionSortAlgorithm : ISortingAlgorithm
{
    public SortingTrace Bake(
        IReadOnlyList<double> values,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        double[] initial = values.ToArray();
        double[] working = (double[])initial.Clone();

        var actions = new List<SortingAction>();

        for (int i = 0; i < working.Length - 1; i++)
        {
            int minIndex = i;

            for (int j = i + 1; j < working.Length; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                actions.Add(new SortingAction(
                    SortingActionKind.Compare,
                    minIndex,
                    j,
                    i,
                    j,
                    working[minIndex],
                    working[j]));

                if (working[j] < working[minIndex])
                    minIndex = j;
            }

            if (minIndex == i)
                continue;

            actions.Add(new SortingAction(
                SortingActionKind.Swap,
                i,
                minIndex,
                i,
                minIndex,
                working[i],
                working[minIndex]));

            (working[i], working[minIndex]) =
                (working[minIndex], working[i]);
        }

        return new SortingTrace(initial, actions.ToArray());
    }
}
