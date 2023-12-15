using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public WFCGrid(List<TCell> possibleCells, int cols, int rows)
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

    /// <summary>
    /// Gets cell by row and column. Returns null if the cell is off the grid.
    ///
    /// Note that grid size and coordinates are int, not uint, both because C# prefers int for indices, and for
    /// convenience of relative addressing.
    /// </summary>
    public PendingCell<TCell> GetCell(int row, int col)
    {
        if (row < 0 || col < 0 || row > Rows - 1 || col > Cols - 1)
        {
            return null;
        }

        return _grid[row][col];
    }

    public List<((int, int, AttachEdge), PendingCell<TCell>)> GetNeighborCells(int row, int col)
    {
        return new List<(int, int, AttachEdge)>
        {
            (row - 1, col, AttachEdge.SOUTH),
            (row + 1, col, AttachEdge.NORTH),
            (row, col - 1, AttachEdge.WEST),
            (row, col + 1, AttachEdge.EAST),
        }
        .Select(location => (location, GetCell(location.Item1, location.Item2)))
        .Where(pair => pair.Item2 != null)
        .ToList();
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
    public delegate List<TCell> Reducer(int row, int col, PendingCell<TCell> cell);

    public WFCGrid<TCell> Grid { get; private set; }

    private Reducer _reducer;
    private Queue<(int, int)> _queue = new Queue<(int, int)>();

    public bool CanIterate { get { return _queue.Count > 0; } }

    public WFCContext(List<TCell> possibleCells, int rows, int cols, Reducer reducer)
    {
        Grid = new WFCGrid<TCell>(possibleCells, rows, cols);
        _reducer = reducer;
    }

    public void SetCell(int row, int col, TCell possibleCell, bool revisitSettled)
    {
        SetCell(row, col, new List<TCell> { possibleCell }, revisitSettled);
    }

    public void SetCell(int row, int col, List<TCell> possibleCells, bool revisitSettled)
    {
        var cell = Grid.GetCell(row, col);
        if (cell == null)
        {
            return;
        }

        cell.PossibleCells = possibleCells;
        var neighbors = Grid.GetNeighborCells(row, col);
        foreach (var neighbor in neighbors)
        {
            if (!neighbor.Item2.IsSettled() || revisitSettled)
            {
                (int neighborRow, int neighborCol, AttachEdge edge) = neighbor.Item1;
                QueueCell(neighborRow, neighborCol);
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
        (int row, int col) coord;
        if (!_queue.TryDequeue(out coord))
        {
            return;
        }

        var cell = Grid.GetCell(coord.row, coord.col);
        if (cell == null)
        {
            return;
        }

        var newTypes = _reducer(coord.row, coord.col, cell);
        if (newTypes != null)
        {
            SetCell(coord.row, coord.col, newTypes, false);
        }
    }

    public bool IsCellQueued(int row, int col)
    {
        return _queue.Any(location => location.Item1 == row && location.Item2 == col);
    }

    private void QueueCell(int row, int col)
    {
        var pair = (row, col);
        if (!_queue.Contains(pair))
        {
            _queue.Enqueue(pair);
        }
    }
}
