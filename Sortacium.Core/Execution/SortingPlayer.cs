namespace Sortacium.Core.Execution;

public sealed class SortingPlayer
{ 
    public int Position { get; private set; }

    public int TotalSteps => _trace.Actions.Count;

    public int Count => _order.Length;

    public int Comparisons { get; private set; }

    public int Swaps { get; private set; }

    public SortingAction? CurrentAction =>
        Position == 0
            ? null
            : _trace.Actions[Position - 1];
    
    private sealed record Checkpoint(
        int Step,
        int[] Order,
        int Comparisons,
        int Swaps);

    private readonly SortingTrace _trace;
    
    private readonly int[] _order;

    private readonly List<Checkpoint> _checkpoints = [];

    private readonly int _checkpointInterval;

    public SortingPlayer(SortingTrace trace, int checkpointInterval = 256)
    {
        _trace = trace;
        _checkpointInterval = checkpointInterval;

        _order = Enumerable
            .Range(0, trace.InitialValues.Count)
            .ToArray();

        BuildCheckpoints();
    }

    public int ElementAt(int position) => _order[position];

    public bool Next()
    {
        if (Position >= TotalSteps)
        {
            return false;
        }

        Apply(_trace.Actions[Position], forward: true);

        Position++;

        return true;
    }

    public bool Previous()
    {
        if (Position <= 0)
        {
            return false;
        }

        Position--;

        Apply(_trace.Actions[Position], forward: false);

        return true;
    }

    private void Apply(SortingAction action, bool forward)
    {
        switch (action.Kind)
        {
            case SortingActionKind.Compare:
                Comparisons += forward ? 1 : -1;
                break;

            case SortingActionKind.Swap:
                (_order[action.Left], _order[action.Right]) =
                    (_order[action.Right], _order[action.Left]);

                Swaps += forward ? 1 : -1;
                break;

            default:
                throw new NotSupportedException(
                    $"Unknown operation: {action.Kind}");
        }
    }

    public void Seek(int target)
    {
        target = Math.Clamp(target, 0, TotalSteps);

        if (target == Position)
        {
            return;
        }

        if (Math.Abs(target - Position) <= _checkpointInterval / 2)
        {
            while (Position < target)
            {
                Next();
            }

            while (Position > target)
            {
                Previous();
            }

            return;
        }

        Checkpoint checkpoint = _checkpoints[target / _checkpointInterval];

        Array.Copy(
            checkpoint.Order,
            _order,
            _order.Length);
        
        Comparisons = checkpoint.Comparisons;
        Position = checkpoint.Step;
        Swaps = checkpoint.Swaps;

        while (Position < target)
        {
            Next();
        }   
    }

    private void BuildCheckpoints()
    {
        int[] order = (int[])_order.Clone();

        int comparisons = 0;
        int swaps = 0;

        _checkpoints.Add(new Checkpoint(
            0,
            (int[])order.Clone(),
            0,
            0));

        for (int step = 0; step < TotalSteps; step++)
        {
            var action = _trace.Actions[step];

            if (action.Kind == SortingActionKind.Compare)
            {
                comparisons++;
            }
            else if (action.Kind == SortingActionKind.Swap)
            {
                swaps++;

                (order[action.Left], order[action.Right]) =
                    (order[action.Right], order[action.Left]);
            }

            if ((step + 1) % _checkpointInterval == 0)
            {
                _checkpoints.Add(new Checkpoint(
                    step + 1,
                    (int[])order.Clone(),
                    comparisons,
                    swaps));
            }
        }
    }
}
