using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace RvB.Grids;

public sealed class Grid<T> : GridBase<T>, IGridSerialize<T>, IEnumerable<(T? Value, GridPos Pos)> {
    private readonly T?[] _grid;

    public delegate void OnCellValueChangedHandler(GridPos pos, T? oldValue, T? newValue);
    public event OnCellValueChangedHandler? OnCellValueChanged;

    /// <summary>
    /// Constructor creating an empty grid with specified dimensions.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public Grid(int width, int height) : this(width, height, (0, 0)) { }

    /// <summary>
    /// Constructor creating an empty grid with specified dimensions and an index offset of (Col, Row).
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="topLeftIndex"></param>
    public Grid(int width, int height, (int Col, int Row) topLeftIndex) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        Height = height;
        Width = width;
        _grid = new T[Height * Width];
        TopLeftIndex = topLeftIndex;
    }

    /// <summary>
    /// Constructor based on a two-dimensional array (rows in first dimension, columns in the second).
    /// </summary>
    /// <param name="grid"></param>
    public Grid(T[,] grid) : this(grid, (0, 0)) { }

    /// <summary>
    /// Constructor based on a two-dimensional array (rows in first dimension, columns in the second) and an index offset of (Col, Row).
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="topLeftIndex">Specifies the row and column index of the top left corner.</param>
    /// <exception cref="ArgumentException"></exception>
    public Grid(T[,] grid, (int Col, int Row) topLeftIndex) {
        ArgumentNullException.ThrowIfNull(grid, nameof(grid));
        if (grid.Length == 0) {
            throw new ArgumentException("Grid has no rows", nameof(grid));
        }
        Height = grid.GetLength(0);
        Width = grid.GetLength(1);
        _grid = new T[Height * Width];
        int index = 0;
        for (var row = 0; row < Height; row++) {
            for (var col = 0; col < Width; col++) {
                _grid[index++] = grid[row, col];
            }
        }
        TopLeftIndex = topLeftIndex;
    }

    public Grid(T[][] grid) : this(grid, (0, 0)) { }

    public Grid(T[][] grid, (int Col, int Row) topLeftIndex) {
        ArgumentNullException.ThrowIfNull(grid, nameof(grid));
        Height = grid.GetLength(0);
        if (Height == 0) {
            throw new ArgumentException("Grid has no rows", nameof(grid));
        }
        Width = grid.Max(r => r.Length);
        if (Width == 0) {
            throw new ArgumentException("Grid has no columns", nameof(grid));
        }
        _grid = new T[Height * Width];
        int index = 0;
        foreach (var row in grid) {
            Array.Copy(row, 0, _grid, index, row.Length);
            index += Width;
        }
        TopLeftIndex = topLeftIndex;
    }

    public Grid(T?[] grid, int width, int height, (int Col, int Row) topLeftIndex) {
        ArgumentNullException.ThrowIfNull(grid, nameof(grid));
        if (height <= 0) {
            throw new ArgumentException("Grid has no rows", nameof(grid));
        }
        if (width <= 0) {
            throw new ArgumentException("Grid has no columns", nameof(grid));
        }
        if (width * height != grid.Length) {
            throw new ArgumentException("Array length does not match width and height", nameof(grid));
        }
        Width = width;
        Height = height;
        _grid = new T[grid.Length];
        Array.Copy(grid, _grid, grid.Length);
        TopLeftIndex = topLeftIndex;
    }

    public Grid(Grid<T> grid) {
        _grid = (T?[])grid._grid.Clone();
        Width = grid.Width;
        Height = grid.Height;
        TopLeftIndex = grid.TopLeftIndex;
    }

    /// <summary>
    /// Creates a shallow copy of the <see cref="Grid{T}"/>.
    /// </summary>
    /// <returns></returns>
    public Grid<T> Clone() {
        return new((T?[])_grid.Clone(), Width, Height, TopLeftIndex);
    }

    public Grid<T> Rotate90CW() {
        var rotated = new Grid<T>(Height, Width, (TopLeftIndex.Row, TopLeftIndex.Col));
        var x = Height - 1;
        for (var r = 0; r < Height; r += 1) {
            for (var c = 0; c < Width; c += 1) {
                rotated._grid[c * Height + x] = _grid[r * Width + c];
            }
            x -= 1;
        }
        return rotated;
    }

    public Grid<T> Rotate90CCW() {
        var rotated = new Grid<T>(Height, Width, (TopLeftIndex.Row, TopLeftIndex.Col));
        for (var r = 0; r < Height; r += 1) {
            var y = Width - 1;
            for (var c = 0; c < Width; c += 1) {
                rotated._grid[y * Height + r] = _grid[r * Width + c];
                y -= 1;
            }
        }
        return rotated;
    }

    public Grid<T> Rotate180() {
        var rotated = new Grid<T>(Width, Height, TopLeftIndex);
        var y = Height - 1;
        for (var r = 0; r < Height; r += 1) {
            var x = Width - 1;
            for (var c = 0; c < Width; c += 1) {
                rotated._grid[y * Width + x] = _grid[r * Width + c];
                x -= 1;
            }
            y -= 1;
        }
        return rotated;
    }

    public Grid<T> FlipHorz() {
        var rotated = new Grid<T>(Width, Height, TopLeftIndex);
        var y = Height - 1;
        for (var r = 0; r < Height; r += 1) {
            for (var c = 0; c < Width; c += 1) {
                rotated._grid[y * Width + c] = _grid[r * Width + c];
            }
            y -= 1;
        }
        return rotated;
    }

    public Grid<T> FlipVert() {
        var rotated = new Grid<T>(Width, Height, TopLeftIndex);
        for (var r = 0; r < Height; r += 1) {
            var x = Width - 1;
            for (var c = 0; c < Width; c += 1) {
                rotated._grid[r * Width + x] = _grid[r * Width + c];
                x -= 1;
            }
        }
        return rotated;
    }

    public T?[] GetData() {
        var data = new T?[Width * Height];
        _grid.CopyTo(data, 0);
        return data;
    }

    public T?[,] GetData2D() {
        var data = new T?[Width, Height];
        ref byte reference = ref MemoryMarshal.GetArrayDataReference(data);
        var span = MemoryMarshal.CreateSpan(ref Unsafe.As<byte, T?>(ref reference), data.Length);
        _grid.CopyTo(span);
        return data;
    }

    public override T? this[int colIndex, int rowIndex] {
        get {
            var (colOffset, rowOffset) = CheckAndMapIndices(colIndex, rowIndex);
            return _grid[rowOffset * Width + colOffset];
        }
        set {
            var (colOffset, rowOffset) = CheckAndMapIndices(colIndex, rowIndex);
            var idx = rowOffset * Width + colOffset;
            if (OnCellValueChanged is not null) {
                if (!_grid[idx]?.Equals(value) ?? value is not null) {
                    var oldValue = _grid[idx];
                    _grid[idx] = value;
                    OnCellValueChanged?.Invoke((colIndex, rowIndex), oldValue, value);
                }
            } else {
                _grid[idx] = value;
            }
        }
    }

    public bool TryGetValue(GridPos pos, out T? value)
        => TryGetValue(pos.Col, pos.Row, out value);

    public bool TryGetValue(int colIndex, int rowIndex, out T? value) {
        colIndex -= TopLeftIndex.Col;
        rowIndex -= TopLeftIndex.Row;
        if ((uint)colIndex < (uint)Width && (uint)rowIndex < (uint)Height) {
            value = _grid[rowIndex * Width + colIndex];
            return true;
        }
        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InBoundsCol(int col)
        => (uint)(col - TopLeftIndex.Col) < (uint)Width;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InBoundsRow(int row)
        => (uint)(row - TopLeftIndex.Row) < (uint)Height;

    public override IEnumerable<(T? Value, int Col)> GetRow(int row) {
        if (!InBoundsRow(row))
            throw new ArgumentOutOfRangeException(nameof(row));
        for (int col = TopLeftIndex.Col; col < TopLeftIndex.Col + Width; ++col) {
            yield return (this[col, row], col);
        }
    }

    public override IEnumerable<(T? Value, int Row)> GetCol(int col) {
        if (!InBoundsCol(col))
            throw new ArgumentOutOfRangeException(nameof(col));
        for (int row = TopLeftIndex.Row; row < TopLeftIndex.Row + Height; row++) {
            yield return (this[col, row], row);
        }
    }

    public IEnumerable<IEnumerable<(T? Value, GridPos Pos)>> Rows {
        get {
            for (int row = TopLeftIndex.Row; row < TopLeftIndex.Row + Height; row++) {
                yield return GetRow(row).Select((vc) => (vc.Value, (vc.Col, row)));
            }
        }
    }

    public IEnumerable<IEnumerable<(T? Value, GridPos Pos)>> Cols {
        get {
            for (int col = TopLeftIndex.Col; col < TopLeftIndex.Col + Width; ++col) {
                yield return GetCol(col).Select(vr => (vr.Value, (col, vr.Row)));
            }
        }
    }

    public int CountAllNeighbors(GridPos pos, Func<T?, bool> selector) {
        var count = 0;
        foreach (var neighbor in Neighbors.AllNeighbors) {
            var neighborPos = pos.Shift(neighbor);
            if (TryGetValue(neighborPos, out var value) && selector(value)) {
                count += 1;
            }
        }
        return count;
    }


    public IEnumerator<(T? Value, GridPos Pos)> GetEnumerator() {
        var gridIdx = 0;
        var rowIdx = TopLeftIndex.Row;
        for (int row = 0; row < Height; row++) {
            for (int col = 0; col < Width; ++col) {
                yield return (_grid[gridIdx++], (col + TopLeftIndex.Col, rowIdx));
            }
            rowIdx += 1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (int Col, int Row) CheckAndMapIndices(int colIndex, int rowIndex) {
        if (!InBoundsCol(colIndex))
            throw new ArgumentOutOfRangeException(nameof(colIndex));
        if (!InBoundsRow(rowIndex))
            throw new ArgumentOutOfRangeException(nameof(rowIndex));
        return (colIndex - TopLeftIndex.Col, rowIndex - TopLeftIndex.Row);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override bool Equals(object? obj) {
        if (obj is Grid<T> grid && Height == grid.Height && Width == grid.Width) {
            return _grid.SequenceEqual(grid._grid);
        }
        return false;
    }

    public override int GetHashCode() {
        HashCode hash = new();
        foreach (var val in _grid)
            hash.Add(val);
        return hash.ToHashCode();
    }

    #region IGridSerialize<T>
    public string SerializeToString() {
        return Serialize(ms => Convert.ToBase64String(ms.ToArray()));
    }

    public static IGrid<T> DeserializeFromString(string byteString) {
        return Grid<T>.Deserialize(Convert.FromBase64String(byteString));
    }

    public string ComputeHash() {
        using var sha = SHA512.Create();
        return Serialize(ms => Convert.ToBase64String(sha.ComputeHash(ms)));
    }
    #endregion

#pragma warning disable IDE0049
    private string Serialize(Func<MemoryStream, string> toString) {
        var type = typeof(T);
        if (type.IsEnum) {
            type = Enum.GetUnderlyingType(typeof(T));
        }
        Func<T, byte[]> GetBytes = type switch {
            Type _ when type == typeof(byte) => FromByte,
            Type _ when type == typeof(Int16) => FromInt16,
            Type _ when type == typeof(Int32) => FromInt32,
            Type _ when type == typeof(Int64) => FromInt64,
            _ => throw new NotImplementedException()
        };

        using var memoryStream = new MemoryStream();
        using var binaryWriter = new BinaryWriter(memoryStream);
        binaryWriter.Write(Width);
        binaryWriter.Write(Height);
        binaryWriter.Write(TopLeftIndex.Col);
        binaryWriter.Write(TopLeftIndex.Row);

        for (int row = TopLeftIndex.Row; row < TopLeftIndex.Row + Height; row++) {
            for (int col = TopLeftIndex.Col; col < TopLeftIndex.Col + Width; ++col) {
                var bytes = GetBytes(this[col, row]!);
                memoryStream.Write(bytes);
            }
        }

        binaryWriter.Close();

        var result = toString(memoryStream);

        //var byteArray = memoryStream.ToArray();
        memoryStream.Close();
        return result;

        static byte[] FromByte(T element) => [(byte)(object)element!];
        static byte[] FromInt16(T element) => BitConverter.GetBytes((Int16)(object)element!);
        static byte[] FromInt32(T element) => BitConverter.GetBytes((Int32)(object)element!);
        static byte[] FromInt64(T element) => BitConverter.GetBytes((Int64)(object)element!);
    }

    private static Grid<T> Deserialize(byte[] bytes) {
        var type = typeof(T);
        if (type.IsEnum) {
            type = Enum.GetUnderlyingType(typeof(T));
        }
        Func<BinaryReader, T> GetBytes = type switch {
            Type _ when type == typeof(byte) => FromByte,
            Type _ when type == typeof(Int16) => FromInt16,
            Type _ when type == typeof(Int32) => FromInt32,
            Type _ when type == typeof(Int64) => FromInt64,
            _ => throw new NotImplementedException()
        };

        using var memoryStream = new MemoryStream(bytes);
        using var binaryReader = new BinaryReader(memoryStream);
        var width = binaryReader.ReadInt32();
        var height = binaryReader.ReadInt32();
        var topLeftIndex = (binaryReader.ReadInt32(), binaryReader.ReadInt32());

        var grid = new Grid<T>(width, height, topLeftIndex);
        var data = new T[width * height];
        for (int row = grid.TopLeftIndex.Row; row < grid.TopLeftIndex.Row + grid.Height; row++) {
            for (int col = grid.TopLeftIndex.Col; col < grid.TopLeftIndex.Col + grid.Width; ++col) {
                grid[col, row] = GetBytes(binaryReader);
            }
        }

        binaryReader.Close();
        memoryStream.Close();

        return grid;

        static T FromByte(BinaryReader reader) => (T)(object)reader.ReadByte();
        static T FromInt16(BinaryReader reader) => (T)(object)BitConverter.ToInt16(reader.ReadBytes(2));
        static T FromInt32(BinaryReader reader) => (T)(object)BitConverter.ToInt32(reader.ReadBytes(4));
        static T FromInt64(BinaryReader reader) => (T)(object)BitConverter.ToInt64(reader.ReadBytes(8));
    }
#pragma warning restore IDE0049
}
