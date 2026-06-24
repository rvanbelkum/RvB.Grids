using RvB.Collections;
using RvB.Graphs;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RvB.Grids;

public sealed class Maze : HugeBitGrid {
    public enum GenerateMethod {
        Backtrack,
        Division,
        Kruskal
    }

    public Maze(int width, int height) : base(width, height) { }

    public static Maze Generate(GenerateMethod method, int width, int height) {
        return method switch {
            GenerateMethod.Backtrack => GenerateBacktrack(width, height),
            GenerateMethod.Division => GenerateDivision(width, height),
            GenerateMethod.Kruskal => GenerateKruskal(width, height),
            _ => throw new NotSupportedException()
        };
    }

    private static Maze GenerateBacktrack(int width, int height) {
        var maze = new Maze(width, height);

        GridPos cur = (1, 1);
        var stack = new Stack<GridPos>();
        stack.Push(cur);
        maze[cur] = true;

        var random = Random.Shared;

        var deltas = new GridPos[4];
        while (stack.TryPeek(out cur)) {
            var (x, y) = cur;

            int targetCount = 0;
            if (x > 2 && !maze[x - 2, y]) {
                deltas[targetCount++] = (-1, 0);
            }
            if (x < maze.Width - 3 && !maze[x + 2, y]) {
                deltas[targetCount++] = (1, 0);
            }
            if (y > 2 && !maze[x, y - 2]) {
                deltas[targetCount++] = (0, -1);
            }
            if (y < maze.Height - 3 && !maze[x, y + 2]) {
                deltas[targetCount++] = (0, 1);
            }

            if (targetCount > 0) {
                var (dc, dr) = deltas[random.Next(targetCount)];
                maze[(x + dc, y + dr)] = true;

                var next = (x + 2 * dc, y + 2 * dr);
                stack.Push(next);
                maze[next] = true;
            } else {
                stack.Pop();
            }
        }
        return maze;
    }

    private static Maze GenerateDivision(int width, int height) {
        var maze = new Maze(width, height);

        for (int x = 1; x < maze.Width - 1; x++) {
            for (int y = 1; y < maze.Height - 1; y++) {
                maze[x, y] = true;
            }
        }

        var rnd = Random.Shared;

        Stack<Rectangle> rectangles = new Stack<Rectangle>();
        Rectangle curRect = new Rectangle(0, 0, maze.Width, maze.Height);
        rectangles.Push(curRect);

        while (rectangles.TryPop(out curRect)) {
            if (curRect.Width <= 3 || curRect.Height <= 3)
                continue;
            if (curRect.Width == 4 && curRect.Height == 4 && curRect.Contains(maze.Width - 2, maze.Height - 2)) {
                //continue;
            }
            var horizontalSplit = (curRect.Width - curRect.Height) switch {
                > 0 => false,
                < 0 => true,
                _ => rnd.Next(2) != 0
            };
            if (horizontalSplit) {
                int splitnumber = 2 + rnd.Next((curRect.Height - 2) / 2) * 2;
                //if (curRect.Y + splitnumber == maze.Height - 2 && curRect.Height != 4 && curRect.Contains(maze.Width - 2, maze.Height - 2)) {
                //    do {
                //        splitnumber = 2 + rnd.Next((curRect.Height - 2) / 2) * 2;
                //    } while (curRect.Y + splitnumber == maze.Height - 2);
                //}
                int opening = 1 + rnd.Next(curRect.Width / 2) * 2;

                var rect1 = new Rectangle(curRect.X, curRect.Y, curRect.Width, splitnumber + 1);
                var rect2 = new Rectangle(curRect.X, curRect.Y + splitnumber, curRect.Width, curRect.Height - splitnumber);

                for (int x = curRect.X; x < curRect.X + curRect.Width; x++) {
                    if (x - curRect.X != opening) {
                        maze[x, curRect.Y + splitnumber] = false;
                    }
                }
                rectangles.Push(rect1);
                rectangles.Push(rect2);
            } else {
                int splitnumber = 2 + rnd.Next((curRect.Width - 2) / 2) * 2;
                //if (curRect.X + splitnumber == maze.Width - 2 && curRect.Width != 4 && curRect.Contains(maze.Width - 2, maze.Height - 2)) {
                //    do {
                //        splitnumber = 2 + rnd.Next((curRect.Width - 2) / 2) * 2;
                //    } while (curRect.X + splitnumber == maze.Width - 2);
                //}
                int opening = 1 + rnd.Next(curRect.Height / 2) * 2;

                var rect1 = new Rectangle(curRect.X, curRect.Y, splitnumber + 1, curRect.Height);
                var rect2 = new Rectangle(curRect.X + splitnumber, curRect.Y, curRect.Width - splitnumber, curRect.Height);

                for (int y = curRect.Y; y < curRect.Y + curRect.Height; y++) {
                    if (y - curRect.Y != opening) {
                        maze[curRect.X + splitnumber, y] = false;
                    }
                }
                rectangles.Push(rect1);
                rectangles.Push(rect2);
            }
            //Draw();
        }
        return maze;
    }

    static void Draw(Maze maze) {
        var (l, t) = Console.GetCursorPosition();
        for (int y = 0; y < maze.Height; y += 1) {
            for (var x = 0; x < maze.Width; x += 1) {
                if (!maze[x, y]) {
                    Console.Write('▒');
                } else {
                    Console.Write('.');
                }
            }
            Console.WriteLine();
        }
        Console.SetCursorPosition(l, t);
    }

    private static Maze GenerateKruskal(int width, int height) {
        var maze = new Maze(width, height);

        // Initialize map & walls
        var map = new Dictionary<GridPos, List<GridPos>>();
        List<GridPos> walls = [];
        for (int x = 1; x < maze.Width - 1; x++) {
            for (int y = 1; y < maze.Height - 1; y++) {
                if ((x & 1) == 1 && (y & 1) == 1) {
                    map.Add((x, y), [(x, y)]);
                    maze[(x, y)] = true;
                } else if ((x & 1) != (y & 1)) {
                    walls.Add((x, y));
                }
            }
        }

#if NET10_0_OR_GREATER
        foreach (var (x, y) in walls.Shuffle()) {
#else
        foreach (var (x, y) in RandomPermutation(walls)) {
#endif
            GridPos c1, c2;
            if ((y & 1) == 1) {
                c1 = (x - 1, y);
                c2 = (x + 1, y);
            } else {
                c1 = (x, y - 1);
                c2 = (x, y + 1);
            }
            if (map.TryGetValue(c1, out var cells1) && map.TryGetValue(c2, out var cells2)) {
                if (!cells1.Equals(cells2)) {
                    maze[x, y] = true;
                    if (cells1.Count > cells2.Count) {
                        cells1.AddRange(cells2);
                        cells2.ForEach(p => map[p] = cells1);
                    } else {
                        cells2.AddRange(cells1);
                        cells1.ForEach(p => map[p] = cells2);
                    }
                }
            } else {
                maze[x, y] = true;
            }
        }
        Dictionary<int, int> histogram = [];
        for (int x = 1; x < maze.Width - 1; x++) {
            for (int y = 1; y < maze.Height - 1; y++) {
                if (map.TryGetValue((x, y), out var cells)) {
                    var size = cells.Count;
                    ref var count = ref CollectionsMarshal.GetValueRefOrAddDefault(histogram, size, out _);
                    count += 1;
                }
            }
        }
        return maze;

#if !NET10_0_OR_GREATER
        static List<T> RandomPermutation<T>(List<T> sequence) {
            var result = sequence.ToList();
            for (int i = 0; i < result.Count - 1; i += 1) {
                int j = Random.Shared.Next(i + 1, result.Count);
                (result[i], result[j]) = (result[j], result[i]);
            }
            return result;
        }
#endif
    }

    public int GetMinimalDistance(GridPos start, GridPos end)
        => GetMinimalDistance(start, end, Directions.CardinalDirections);

    public int GetMinimalDistance(GridPos start, GridPos end, GridPosInternal[] directions) {
        if (!InBounds(start)) {
            throw new ArgumentOutOfRangeException(nameof(start));
        }
        if (!InBounds(end)) {
            throw new ArgumentOutOfRangeException(nameof(end));
        }
        var visited = new LightWeightBitArray((ulong)(Width * Height));
        PriorityQueue<GridPosInternal, int> toDo = new();

        // As a performance optimization, we will only work with the internal rows and cols.
        GridPosInternal realStart = ((uint)(start.Col - TopLeftIndex.Col), (uint)(start.Row - TopLeftIndex.Row));
        GridPosInternal realEnd = ((uint)(end.Col - TopLeftIndex.Col), (uint)(end.Row - TopLeftIndex.Row));

        var array = _array;
        var width = (uint)Width;
        var height = (uint)Height;

        var pathDistance = 0;
        toDo.Enqueue(realStart, 0);
        while (toDo.TryDequeue(out var pos, out var distance)) {
            var key = (ulong)(pos.Col + Width * pos.Row);
            if (!visited.SetIfNotSet(key))
                continue;
            if (pos == realEnd) {
                pathDistance = distance;
                break;
            }
            for (var d = 0; d < directions.Length; d += 1) {
                var (dc, dr) = directions[d];
                var (col, row) = (pos.Col + dc, pos.Row + dr);
                if (col < width && row < height && array[row][col]) {
                    toDo.Enqueue((col, row), distance + 1);
                }
            }
        }
        return pathDistance;
    }

    internal enum Direction : byte {
        West = 0,
        North = 1,
        South = 2,
        East = 3,
    }

    public readonly struct Path : IEnumerable<GridPos> {
        private readonly BitArray _path;
        private readonly GridPos _topLeftPos;

        public static Path Empty { get; } = new();

        private readonly GridPosInternal _realStart;

        public int Length { get; }

        public Path() {
            _path = new BitArray(0);
            _realStart = default;
            Length = 0;
        }

        internal Path(GridPos topLeftPos, GridPosInternal start, int length, IEnumerable<uint> reversedPath) {
            ArgumentOutOfRangeException.ThrowIfNegative(length);

            _path = new BitArray(2 * length);
            _topLeftPos = topLeftPos;
            _realStart = start;
            Length = length;

            var lastIndex = 2 * length;
            foreach (var direction in reversedPath) {
                _path[--lastIndex] = (direction & 1) != 0;
                _path[--lastIndex] = (direction & 2) != 0;
            }
            if (lastIndex != 0) {
                throw new InvalidOperationException();
            }
        }

        public IEnumerator<GridPos> GetEnumerator() {
            var (colOffset, rowOffset) = _topLeftPos;
            var pos = _realStart;
            yield return ((int)pos.Col + colOffset, (int)pos.Row + rowOffset);
            for (var i = 0; i < _path.Length; i += 2) {
                var direction = (uint)((_path[i] ? 2 : 0) + (_path[i + 1] ? 1 : 0));
                pos = Node.Move(pos, direction);
                yield return ((int)pos.Col + colOffset, (int)pos.Row + rowOffset);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal readonly struct Node : IEquatable<Node> {
        private static readonly GridPosInternal[] s_directions = [Directions.West, Directions.North, Directions.South, Directions.East];

        private readonly uint _col;
        private readonly uint _row;

        public static GridPosInternal Move(GridPosInternal pos, uint dir) {
            var (dc, dr) = s_directions[dir];
            return (pos.Col + dc, pos.Row + dr);
        }

        public uint Col {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return _col & 0x7FFFFFFF;
            }
        }

        public uint Row {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                return _row & 0x7FFFFFFF;
            }
        }

        public GridPosInternal Pos => (Col, Row);

        public GridPosInternal Parent {
            get {
                var (dc, dr) = s_directions[(byte)Direction];
                return (Col + dc, Row + dr);
            }
        }

        public uint Direction => ((_row >> 31) << 1) | (_col >> 31);

        public Node(GridPosInternal pos, GridPosInternal parent) {
            /*
             *  (-1,  0) => Direction.Left,   0
             *  ( 0, -1) => Direction.Up,     1
             *  ( 0,  1) => Direction.Down,   2
             *  ( 1,  0) => Direction.Right,  3
             */
            var id = (3 * (parent.Col - pos.Col) + (parent.Row - pos.Row) + 3) / 2;
            _col = (id & 1) != 0 ? pos.Col | 0x80000000 : pos.Col;
            _row = (id & 2) != 0 ? pos.Row | 0x80000000 : pos.Row;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is Node node && Equals(node);

        public bool Equals(Node other) => Col == other.Col && Row == other.Row;

        public override int GetHashCode() {
            return HashCode.Combine(Col, Row);
        }
    }

    public Path GetMinimalPath(GridPos start, GridPos end)
        => GetMinimalPath(start, end, Directions.CardinalDirections);

    public Path GetMinimalPath(GridPos start, GridPos end, GridPosInternal[] directions) {
        HashSet<Node> visited = [];
        PriorityQueue<Node, int> toDo = new();

        // As a performance optimization, we will only work with the internal rows and cols.
        GridPosInternal realStart = ((uint)(start.Col - TopLeftIndex.Col), (uint)(start.Row - TopLeftIndex.Row));
        GridPosInternal realEnd = ((uint)(end.Col - TopLeftIndex.Col), (uint)(end.Row - TopLeftIndex.Row));

        var array = _array;
        var width = (uint)Width;
        var height = (uint)Height;

        toDo.Enqueue(new(realStart, default), 0);
        var path = Path.Empty;
        while (toDo.TryDequeue(out var item, out var distance)) {
            if (!visited.Add(item))
                continue;
            var pos = item.Pos;
            if (pos == realEnd) {
                path = new Path(TopLeftIndex, realStart, distance, ReversedDirections(visited, realStart, realEnd));
                break;
            }
            for (var d = 0; d < directions.Length; d += 1) {
                var (dc, dr) = directions[d];
                var (col, row) = (pos.Col + dc, pos.Row + dr);
                if (col < width && row < height && array[row][col]) {
                    toDo.Enqueue(new((col, row), pos), distance + 1);
                }
            }
        }
        return path;

        static IEnumerable<uint> ReversedDirections(HashSet<Node> visited, GridPosInternal start, GridPosInternal end) {
            var node = new Node(end, end);
            while (node.Pos != start) {
                _ = visited.TryGetValue(node, out node);
                node = new(node.Parent, node.Pos);
                yield return node.Direction;
            }
        }
    }
}
