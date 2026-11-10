namespace Sortacium.Core.Execution;

public sealed class SortingTrace
{
    public IReadOnlyList<double> InitialValues { get; }
    public IReadOnlyList<SortingAction> Actions { get; }

    public SortingTrace(
        double[] initialValues,
        SortingAction[] actions)
    {
        InitialValues = Array.AsReadOnly(
            (double[])initialValues.Clone());

        Actions = Array.AsReadOnly(
            (SortingAction[])actions.Clone());
    }
}
