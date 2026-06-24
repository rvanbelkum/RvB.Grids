using System.Collections;

namespace RvB.Grids;

public enum SparseGridBehavior {
    FixedSize,
    Autogrow,
    AutogrowKeepSquare
}

public sealed class SparseGrid<TElement> : GridBase<TElement>, IEquatable<SparseGrid<TElement>>, IEnumerable<(GridPos Pos, TElement? Value)> {
    private readonly Dictionary<GridPos, TElement?> _grid;
    private GridPos _topLeft;
    private GridPos _bottomRight;

    public SparseGrid(int size)
        : this((0, 0), (size - 1, size - 1)) {
    }

    public SparseGrid(int width, int height)
        : this((0, 0), (width - 1, height - 1)) {
    }

    public SparseGrid(GridPos topLeft, int size)
        : this(topLeft, (topLeft.Col + size - 1, topLeft.Row + size - 1)) {
    }

    public SparseGrid(GridPos topLeft, GridPos bottomRight) {
        if (topLeft.Col > bottomRight.Col || topLeft.Row > bottomRight.Row)
            throw new ArgumentException("Incorrect grid coordinates");
        _topLeft = topLeft;
        _bottomRight = bottomRight;
        _grid = new(Width * Height);
    }

    public SparseGrid(SparseGrid<TElement> grid) {
        _topLeft = grid._topLeft;
        _bottomRight = grid._bottomRight;
        AutoGrow = grid.AutoGrow;
        _grid = new(grid._grid);
    }

    public SparseGrid<TElement> Clone() => new(this);

    public void Clear() => _grid.Clear();

    public GridPos TopLeft => _topLeft;

    public GridPos BottomRight => _bottomRight;

    public override int Width => _bottomRight.Col - _topLeft.Col + 1;

    public override int Height => _bottomRight.Row - _topLeft.Row + 1;

    public SparseGridBehavior AutoGrow { get; set; } = SparseGridBehavior.Autogrow;

    public bool TryGetValue(GridPos index, out TElement? element) {
        return (_grid.TryGetValue(index, out element));
    }

    public override TElement? this[int col, int row] {
        get {
            CheckBounds((col, row));
            if (_grid.TryGetValue((col, row), out var element))
                return element;
            return default;
        }
        set {
            CheckBounds((col, row));
            _grid[(col, row)] = value;
        }
    }

    public bool Remove(int col, int row)
        => _grid.Remove((col, row));

    public bool Remove(GridPos pos)
        => _grid.Remove(pos);

    public override IEnumerable<(TElement? Value, int Row)> GetCol(int col) {
        for (var row = _topLeft.Row; row <= _bottomRight.Row; row += 1) {
            yield return (this[col, row], row);
        }
    }

    public override IEnumerable<(TElement? Value, int Col)> GetRow(int row) {
        for (var col = _topLeft.Col; col <= _bottomRight.Col; col += 1) {
            yield return (this[col, row], col);
        }
    }

    public bool Equals(SparseGrid<TElement>? other) {
        if (other is null
            || _grid.Count != other._grid.Count
            || _topLeft != other._topLeft
            || _bottomRight != other._bottomRight
            || AutoGrow != other.AutoGrow) {
            return false;
        }
        return _grid.SequenceEqual(other._grid);
    }

    public override bool Equals(object? obj) {
        if (obj is not null && obj is SparseGrid<TElement> other)
            return Equals(other);
        return false;
    }

    public override int GetHashCode() {
        HashCode hash = new();
        foreach (var val in _grid) {
            hash.Add(val);
        }
        hash.Add(_topLeft);
        hash.Add(_bottomRight);
        hash.Add(AutoGrow);
        return hash.ToHashCode();
    }

    public static bool operator ==(SparseGrid<TElement> left, SparseGrid<TElement> right) {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(SparseGrid<TElement> left, SparseGrid<TElement> right)
        => !(left == right);

    private void CheckBounds(GridPos index) {
        if (index.Col < _topLeft.Col || index.Row < _topLeft.Row) {
            if (AutoGrow == SparseGridBehavior.FixedSize)
                throw new IndexOutOfRangeException();
            var colDiff = int.Max(0, _topLeft.Col - index.Col);
            var rowDiff = int.Max(0, _topLeft.Row - index.Row);
            if (AutoGrow == SparseGridBehavior.AutogrowKeepSquare) {
                colDiff = rowDiff = int.Max(colDiff, rowDiff);
            }
            _topLeft = (_topLeft.Col - colDiff, _topLeft.Row - rowDiff);
        }
        if (index.Col > _bottomRight.Col || index.Row > _bottomRight.Row) {
            if (AutoGrow == SparseGridBehavior.FixedSize)
                throw new IndexOutOfRangeException();
            var colDiff = int.Max(0, index.Col - _bottomRight.Col);
            var rowDiff = int.Max(0, index.Row - _bottomRight.Row);
            if (AutoGrow == SparseGridBehavior.AutogrowKeepSquare) {
                colDiff = rowDiff = int.Max(colDiff, rowDiff);
            }
            _bottomRight = (_bottomRight.Col + colDiff, _bottomRight.Row + rowDiff);
        }
    }

    public override bool InBounds(int col, int row) {
        if (AutoGrow == SparseGridBehavior.FixedSize) {
            return col >= _topLeft.Col && row >= _topLeft.Row
                && col <= _bottomRight.Col && row <= _bottomRight.Row;
        } else {
            return true;
        }
    }

    public IEnumerator<((int Col, int Row) Pos, TElement? Value)> GetEnumerator() {
        foreach (var (pos, value) in _grid) {
            yield return (pos, value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /*
    public override byte[] Serialize() {

    }

    public static new Grid<TElement> Deserialize(byte[] bytes) {

    }
    */
}
