# RvB.Grids
Various grid implementations:
- `Grid<T>`: rectangular grid of `T` elements
- `HugeBitGrid`: designed for large (maze like) grids
- `Maze`: supports huge mazes.
- `HexagonRectGrid<T>`: rectangular grid of even sided hexagons
- `JaggedGrid`: grid which supports varying widths per row
- `EnumGrid<TRow, TCol, TElement>`: rectangular grid in which rows and columns are identified by an `enum`. `TCol` and `TRow` must be enums.
- `SparseGrid<T>`: specifically suited for large but sparse grids.
- ...
