using System.Collections.Generic;
using System.Linq;

public struct GridLocation
{
    public int row;
    public int col;

    public GridLocation(int row, int col)
    {
        this.row = row;
        this.col = col;
    }

    public bool Equals(GridLocation other)
    {
        return other.row == this.row && other.col == this.col;
    }

    public override bool Equals(object obj)
    {
        if (obj is GridLocation)
        {
            return Equals((GridLocation)obj);
        }

        return false;
    }

    public static bool operator ==(GridLocation lhs, GridLocation rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(GridLocation lhs, GridLocation rhs)
    {
        return !lhs.Equals(rhs);
    }

    public override int GetHashCode()
    {
        return $"{row},{col}".GetHashCode();
    }

    public override string ToString()
    {
        return $"({row},{col})";
    }

    public static implicit operator GridLocation((int row, int col) tuple)
    {
        return new GridLocation(tuple.row, tuple.col);
    }

    public static implicit operator (int row, int col)(GridLocation loc)
    {
        return (loc.row, loc.col);
    }

    public IEnumerable<(GridLocation loc, AttachEdge edge)> GetAllNeighborLocations()
    {
        return new List<(GridLocation, AttachEdge)>
        {
            ((row - 1, col), AttachEdge.SOUTH),
            ((row + 1, col), AttachEdge.NORTH),
            ((row, col - 1), AttachEdge.WEST),
            ((row, col + 1), AttachEdge.EAST),
        };
    }
}

public class PendingCell<TCell>
{
    public List<TCell> PossibleCells { get; set; }

    private int _maxCellTypes;

    public PendingCell(List<TCell> possibleCells)
    {
        PossibleCells = new List<TCell>(possibleCells);
        _maxCellTypes = PossibleCells.Count;
    }

    public bool IsSettled()
    {
        return PossibleCells.Count <= 1;
    }

    public bool IsUntouched()
    {
        return PossibleCells.Count == _maxCellTypes;
    }
}


public class WFCGrid<TCell>
{
    private List<List<PendingCell<TCell>>> _grid;

    public int Rows { get; private set; }
    public int Cols { get; private set; }

    public WFCGrid(List<TCell> possibleCells, int rows, int cols)
    {
        Rows = rows;
        Cols = cols;

        _grid = new List<List<PendingCell<TCell>>>();
        for (int row = 0; row < rows; row++)
        {
            var rowList = new List<PendingCell<TCell>>();

            for (int col = 0; col < cols; col++)
            {
                rowList.Add(new PendingCell<TCell>(possibleCells));
            }

            _grid.Add(rowList);
        }
    }

    public List<List<PendingCell<TCell>>> GetCells()
    {
        return _grid;
    }

    /// <summary>
    /// Gets cell by row and column. Returns null if the cell is off the grid.
    ///
    /// Note that grid size and coordinates are int, not uint, both because C# prefers int for indices, and for
    /// convenience of relative addressing.
    /// </summary>
    public PendingCell<TCell> GetCell(GridLocation loc)
    {
        if (loc.row < 0 || loc.col < 0 || loc.row > Rows - 1 || loc.col > Cols - 1)
        {
            return null;
        }

        return _grid[loc.row][loc.col];
    }

    public IEnumerable<((GridLocation loc, AttachEdge edge), PendingCell<TCell>)> GetLiveNeighborCells(GridLocation loc)
    {
        return GetAllNeighborCells(loc).Where(pair => pair.Item2 != null);
    }

    public IEnumerable<((GridLocation loc, AttachEdge edge), PendingCell<TCell>)> GetAllNeighborCells(GridLocation loc)
    {
        return loc.GetAllNeighborLocations().Select(location => (location, GetCell(location.Item1)));
    }

    public bool AllSettled()
    {
        return _grid.All(row => row.All(cell => cell.IsSettled()));
    }
}

/// <summary>
/// Base Wave Function Collapse logic.
///
/// Specifically, it wraps up a WFCGrid and contains the logic to visit neighboring cells as needed after possible cell
/// types are updated through SetCell. The general flow is to use additional logic to place initial cells, then flip-
/// flop between calls to IterateComplete and additional external population logic until the grid is settled.
///
/// Note that external mutations to cells should be done through SetCell, which will enqueue pending neighbors for
/// reduction. Types returned through the Reducer are automatically applied this way.
/// </summary>
public class WFCContext<TCell>
{
    /// <summary>
    /// Given a cell, return an updated list of possible types (usually based on neighbors). Returning null means no
    /// change.
    /// </summary>
    public delegate List<TCell> Reducer(GridLocation loc, PendingCell<TCell> cell);

    public WFCGrid<TCell> Grid { get; private set; }

    private Reducer _reducer;
    private Queue<GridLocation> _queue = new Queue<GridLocation>();

    public bool CanIterate { get { return _queue.Count > 0; } }

    public WFCContext(List<TCell> possibleCells, int rows, int cols, Reducer reducer)
    {
        Grid = new WFCGrid<TCell>(possibleCells, rows, cols);
        _reducer = reducer;
    }

    public void SetCell(GridLocation loc, TCell possibleCell, bool revisitSettled)
    {
        SetCell(loc, new List<TCell> { possibleCell }, revisitSettled);
    }

    public void SetCell(GridLocation loc, List<TCell> possibleCells, bool revisitSettled)
    {
        var cell = Grid.GetCell(loc);
        if (cell == null)
        {
            return;
        }

        cell.PossibleCells = possibleCells;
        var neighbors = Grid.GetLiveNeighborCells(loc);
        foreach (var neighbor in neighbors)
        {
            if (!neighbor.Item2.IsSettled() || revisitSettled)
            {
                (GridLocation neighborLoc, AttachEdge edge) = neighbor.Item1;
                QueueCell(neighborLoc);
            }
        }
    }

    public void IterateComplete()
    {
        while (CanIterate)
        {
            Iterate();
        }
    }

    public bool AllSettled()
    {
        return Grid.AllSettled();
    }

    public void Iterate()
    {
        GridLocation coord;
        if (!_queue.TryDequeue(out coord))
        {
            return;
        }

        var cell = Grid.GetCell(coord);
        if (cell == null)
        {
            return;
        }

        var newTypes = _reducer(coord, cell);
        if (newTypes != null)
        {
            SetCell(coord, newTypes, false);
        }
    }

    public bool IsCellQueued(GridLocation loc)
    {
        return _queue.Any(location => location == loc);
    }

    private void QueueCell(GridLocation location)
    {
        if (!_queue.Contains(location))
        {
            _queue.Enqueue(location);
        }
    }
}
