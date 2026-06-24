global using GridPos = (int Col, int Row);
global using GridPosInternal = (uint Col, uint Row);
using System.Runtime.CompilerServices;

namespace RvB.Grids;

public static class GridPosExtension {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GridPos Shift(this GridPos pos, (int Col, int Row) delta)
        => (pos.Col + delta.Col, pos.Row + delta.Row);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GridPos Shift(this GridPos pos, int col, int row)
        => (pos.Col + col, pos.Row + row);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ManhattanDistance(this GridPos p, GridPos q)
        => Math.Abs(p.Col - q.Col) + Math.Abs(p.Row - q.Row);
}
