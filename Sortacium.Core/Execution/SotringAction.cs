namespace Sortacium.Core.Execution;

public readonly record struct SortingAction(
    SortingActionKind Kind,
    ushort Left,
    ushort Right,
    ushort I,
    ushort J,
    float LeftValue,
    float RightValue)
{
    public SortingCodePoint CodePoint
        => Kind == SortingActionKind.Compare
            ? SortingCodePoint.CompareAdjacent
            : SortingCodePoint.SwapAdjacent;
}
