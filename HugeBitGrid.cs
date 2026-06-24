using RvB.Collections;
using System.Collections;
using System.Runtime.CompilerServices;

namespace RvB.Grids;

public class HugeBitGrid : GridBase<bool> {
    protected readonly LightWeightBitArray[] _array;

    public HugeBitGrid(int width, int height) : this(width, height, (0, 0)) { }

    public HugeBitGrid(int width, int height, GridPos topLeftIndex) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        Height = height;
        Width = width;
        _array = new LightWeightBitArray[height];
        for (var y = 0; y < height; y++) {
            _array[y] = new(width);
        }
        TopLeftIndex = topLeftIndex;
    }

    public override bool this[int col, int row] {
        get {
            var (colOffset, rowOffset) = CheckAndMapIndices(col, row);
            return _array[rowOffset][colOffset];
        }
        set {
            var (colOffset, rowOffset) = CheckAndMapIndices(col, row);
            _array[rowOffset][colOffset] = value;
        }
    }

    public override IEnumerable<(bool Value, int Col)> GetRow(int row) {
        if (!InBoundsRow(row))
            throw new ArgumentOutOfRangeException(nameof(row));
        row -= TopLeftIndex.Row;
        var width = Width;
        var topLeftIndexCol = TopLeftIndex.Col;
        for (int col = 0; col < width; ++col) {
            yield return (_array[row][col], col - topLeftIndexCol);
        }
    }

    public override IEnumerable<(bool Value, int Row)> GetCol(int col) {
        if (!InBoundsCol(col))
            throw new ArgumentOutOfRangeException(nameof(col));
        col -= TopLeftIndex.Col;
        var height = Height;
        var topLeftIndexRow = TopLeftIndex.Row;
        for (int row = 0; row < height; row++) {
            yield return (_array[row][col], row - topLeftIndexRow);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InBoundsCol(int col)
        => (uint)(col - TopLeftIndex.Col) < (uint)Width;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InBoundsRow(int row)
        => (uint)(row - TopLeftIndex.Row) < (uint)Height;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private GridPosInternal CheckAndMapIndices(int colIndex, int rowIndex) {
        if (!InBoundsCol(colIndex))
            throw new ArgumentOutOfRangeException(nameof(colIndex));
        if (!InBoundsRow(rowIndex))
            throw new ArgumentOutOfRangeException(nameof(rowIndex));
        return ((uint)(colIndex - TopLeftIndex.Col), (uint)(rowIndex - TopLeftIndex.Row));
    }
}
