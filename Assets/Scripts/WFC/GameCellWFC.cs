using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameCellWFC
{
    private List<GameCell> _gameCells;

    private int _rows;
    private int _cols;

    private class WCFCell
    {
        public GameCell BaseCell { get; private set; }
        public int InnerRow { get; private set; }
        public int InnerCol { get; private set; }
        public bool CanBeRandom { get; private set; }
        public List<CellAttachPoint> AttachPoints { get; private set; }

        public static List<WCFCell> FromGameCell(GameCell gameCell)
        {
            return gameCell.Footprint.Select(footprint => {
                return new WCFCell
                {
                    BaseCell = gameCell,
                    InnerRow = footprint.row,
                    InnerCol = footprint.col,
                    CanBeRandom = false, // TODO: Add prop
                    AttachPoints = gameCell.AttachPoints.Where(
                        point => point.row == footprint.row && point.col == footprint.col
                    ).ToList(),
                };
            }).ToList();
        }
    }

    private List<WCFCell> _allCellTypes;
    private List<WCFCell> _randomCellTypes;
    private WFCContext<WCFCell> _wfc;

    public GameCellWFC(int rows, int cols, List<GameCell> gameCells)
    {
        _rows = rows;
        _cols = cols;
        _gameCells = gameCells;

        _allCellTypes = _gameCells.Select(cell => WCFCell.FromGameCell(cell)).SelectMany(x => x).ToList();
        _randomCellTypes = _allCellTypes.Where(t => t.CanBeRandom).ToList();
        _wfc = new WFCContext<WCFCell>(_randomCellTypes, rows, _cols, (row, col, cell) => Reduce(row, col, cell));
    }

    private List<WCFCell> Reduce(int row, int col, PendingCell<WCFCell> cell)
    {
        // First, check if any neighbors are parts of a bigger cell that would include this one, and narrow to that
        // type if needed. If that part isn't in our list of possibilities, that's a problem. Also remove any multi-cell
        // possibilities that wouldn't work next to current neighbors.

        // Then, reduce based on neighbor connections.
        //   If a neighbor _must_ connect, reduce to only connected types
        //   If a neighbor _may_ connect, but retains possible non-connection, don't reduce

        // That's for each neighbor, though, so if if the cell was narrowed down to:
        //   connect north, east
        //   connect west, south
        // and we now discovered east was blocked, this cell would only be open to options that connect west, south.

        // In that case, when the northern neighbor is re-visited as a result of this update, it will then reduce to
        // options that don't connect south. The set of initial possibilities should include "empty" roads, all types of
        // intersections, and non-connected cells (city greebling?).

        // Try to keep logic here focused on hard "cans" and "cannots." For example, the "filler" logic might do
        // something like define roads, and this reacts to update the surrounding cells to foot-only connections.

        return null;
    }
}
