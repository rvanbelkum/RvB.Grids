using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace RvB.Grids;

/// <summary>
/// A grid indexed by enumerated types instead of integers
/// </summary>
public class EnumGrid<TCol, TRow, TElement> : IEnumerable<TElement?> where TCol : struct, Enum where TRow : struct, Enum {
    private static readonly int s_arraySize = 0;
    private static readonly int s_lowerBound;
    private static readonly int s_rowSize = 0;
    private static readonly Dictionary<TCol, int>? s_enumCol2Index;
    private static readonly Dictionary<TRow, int>? s_enumRow2Index;
    private static readonly Func<TCol, TRow, int>? s_indexer;
    private static readonly TCol[] s_enumColValues;
    private static readonly TRow[] s_enumRowValues;

    private readonly TElement?[]? _array;
    private readonly Dictionary<(TCol, TRow), TElement?>? _dictionary;

    static EnumGrid() {
        s_enumColValues = [.. Enum.GetValues<TCol>().Order()];
        s_enumRowValues = [.. Enum.GetValues<TRow>().Order()];
        if (s_enumColValues.Length == 0 || s_enumRowValues.Length == 0)
            return;

        var minCol = Convert.ToInt64(s_enumColValues[0]);
        var maxCol = Convert.ToInt64(s_enumColValues[^1]);
        var colRangeSize = maxCol - minCol + 1;

        var minRow = Convert.ToInt64(s_enumRowValues[0]);
        var maxRow = Convert.ToInt64(s_enumRowValues[^1]);
        var rowRangeSize = maxRow - minRow + 1;

        var totalRangeSize = colRangeSize * rowRangeSize;

        if (minCol >= int.MinValue && maxCol <= int.MaxValue
            && minRow >= int.MinValue && maxRow <= int.MaxValue
            && totalRangeSize < s_enumColValues.Length * s_enumRowValues.Length * 2L) {
            s_arraySize = (int)totalRangeSize;
            s_rowSize = (int)colRangeSize;
            s_lowerBound = (int)(minCol + minRow * s_rowSize);
            s_indexer = (col, row) => Convert.ToInt32(col) + Convert.ToInt32(row) * s_rowSize - s_lowerBound;
        } else if (s_enumColValues.Length * s_enumRowValues.Length < 100_000) {
            s_enumCol2Index = [];
            for (var i = 0; i < s_enumColValues.Length; i += 1) {
                s_enumCol2Index[s_enumColValues[i]] = i;
            }
            s_enumRow2Index = [];
            for (var i = 0; i < s_enumRowValues.Length; i += 1) {
                s_enumRow2Index[s_enumRowValues[i]] = i;
            }
            s_arraySize = s_enumColValues.Length * s_enumRowValues.Length;
            s_rowSize = s_enumColValues.Length;
            s_indexer = (col, row) => s_enumCol2Index[col] + s_enumRow2Index[row] * s_rowSize;
        }
    }

    [MemberNotNullWhen(true, nameof(s_indexer), nameof(_array))]
    [MemberNotNullWhen(false, nameof(_dictionary))]
    private bool HasArray => s_indexer is not null;

    /// <summary>
    /// Creates the initial array, populated with the defaults for TElement
    /// </summary>
    public EnumGrid() {
        if (s_arraySize > 0) {
            _array = (TElement[])Array.CreateInstance(typeof(TElement), s_arraySize);
        } else {
            _dictionary = new(s_enumColValues.Length * s_enumRowValues.Length);
        }
    }

    private TElement? GetValue(TCol col, TRow row) {
        if (HasArray) {
            return _array[s_indexer(col, row)];
        }
        if (!_dictionary.TryGetValue((col, row), out var element)) {
            return default;
        }
        return element;
    }

    private void SetValue(TCol col, TRow row, TElement? value) {
        if (HasArray) {
            _array[s_indexer(col, row)] = value;
        } else {
            _dictionary[(col, row)] = value;
        }
    }

    /// <summary>
    /// Gets the element by enumerated type
    /// </summary>
    public TElement? this[TCol col, TRow row] {
        get => GetValue(col, row);
        set => SetValue(col, row, value);
    }

    public IEnumerable<TElement?> GetCol(TCol col) {
        foreach (var row in s_enumRowValues)
            yield return this[col, row];
    }

    public IEnumerable<TElement?> GetRow(TRow row) {
        foreach (var col in s_enumColValues)
            yield return this[col, row];
    }

    /// <summary>
    /// Gets a generic enumerator
    /// </summary>
    public IEnumerator<TElement?> GetEnumerator() {
        foreach (var row in s_enumRowValues) {
            foreach (var col in s_enumColValues) {
                yield return GetValue(col, row);
            }
        }
    }

    public override bool Equals(object? obj) {
        if (obj is EnumGrid<TRow, TCol, TElement> grid) {
            if (_array is not null && grid._array is not null)
                return _array.SequenceEqual(grid._array);
            return _array is null && grid._array is null;
        }
        return false;
    }

    public override int GetHashCode() {
        if (_array is null)
            return 0;
        HashCode hash = new();
        foreach (var val in _array) {
            hash.Add(val);
        }
        return hash.ToHashCode();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
