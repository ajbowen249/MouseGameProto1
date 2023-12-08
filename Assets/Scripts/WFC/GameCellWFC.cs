using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class WFCCell
{
    public GameCell BaseCell { get; private set; }
    public int InnerRow { get; private set; }
    public int InnerCol { get; private set; }
    public bool CanBeRandom { get; private set; }
    public bool IsSubCell { get; private set; }
    public List<AbsoluteAttachPoint> AttachPoints { get; private set; }

    public static List<WFCCell> FromGameCell(GameCell gameCell)
    {
        // Each game cell can take up multiple "cells" in the grid. The "footprint" defines which ones, so this splits
        // the prefab into at least one WFCCell per inner "cell."
        return gameCell.Footprint.SelectMany(footprint =>
        {
            // Attach points are also assigned a cell, so we also want to narrow this down to only include attach points
            // in this quadrant.
            var relevantAttachPoints = gameCell.DistinctAttachPoints.Where(
                point => point.row == footprint.row && point.col == footprint.col
            ).ToList();

            var optionalPoints = new List<AbsoluteAttachPoint>();
            var requiredPoints = new List<AbsoluteAttachPoint>();

            foreach (var point in relevantAttachPoints)
            {
                var absolutePoint = new AbsoluteAttachPoint
                {
                    edge = point.edge,
                    modeType = point.mode.type,
                };

                (point.mode.isOptional ? optionalPoints : requiredPoints).Add(absolutePoint);
            }

            // Attach points can be optional. So, to make the reducer function simpler, generate the full set of
            // possibilities as a set of cell types. It should be rare for this to generate a ton of options. The full
            // 4-way blank cell is the worst one.

            var absoluteSets = new List<List<AbsoluteAttachPoint>>();
            var numVariants = Math.Pow(2, optionalPoints.Count);

            for (uint i = 0; i < numVariants; i++)
            {
                var points = new List<AbsoluteAttachPoint>(requiredPoints);

                for (int bit = 0; bit < optionalPoints.Count; bit++)
                {
                    var isSet = (i >> bit & 1) != 0;
                    if (isSet)
                    {
                        points.Add(optionalPoints[bit]);
                    }
                }

                absoluteSets.Add(points);
            }

            return absoluteSets.Select(attachPoints => new WFCCell
            {
                BaseCell = gameCell,
                InnerRow = footprint.row,
                InnerCol = footprint.col,
                CanBeRandom = gameCell.CanBeRandom,
                IsSubCell = gameCell.Footprint.Count > 1,
                AttachPoints = attachPoints,
            });
        }).ToList();
    }
}

public struct AbsoluteAttachPoint
{
    public AttachEdge edge;
    public AttachModeType modeType;
}

public class GameCellWFC
{
    private List<GameCell> _gameCells;

    private int _rows;
    private int _cols;

    private List<WFCCell> _allCellTypes;
    private List<WFCCell> _randomCellTypes;
    private WFCContext<WFCCell> _wfc;

    public GameCellWFC(int rows, int cols, List<GameCell> gameCells)
    {
        _rows = rows;
        _cols = cols;
        _gameCells = gameCells;

        _allCellTypes = _gameCells.Select(cell => WFCCell.FromGameCell(cell)).SelectMany(x => x).ToList();
        _randomCellTypes = _allCellTypes.Where(t => t.CanBeRandom).ToList();
        _wfc = new WFCContext<WFCCell>(_randomCellTypes, rows, _cols, (row, col, cell) => Reduce(row, col, cell));
    }

    public void Generate()
    {
        PlaceFixedCells();
        _wfc.IterateComplete();
    }

    public WFCGrid<WFCCell> GetGrid()
    {
        return _wfc.Grid;
    }

    private void PlaceFixedCells()
    {
        var BlankCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "BlankCell");
        var HomeCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "HomeCell");
        var YardCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "YardCell");
        var BlockedRoadCell = _allCellTypes.Find(cell =>
            cell.BaseCell.gameObject.name == "BlockedRoadCell" && cell.InnerRow == 0 && cell.InnerCol == 0
        );

        var HotDogStandCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "HotDogStandCell");
        var CheeseStoreCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "CheeseStoreCell");

        _wfc.SetCell(0, 5, HomeCell, true);
        _wfc.SetCell(1, 5, YardCell, true);
        _wfc.SetCell(2, 5, BlockedRoadCell, true);
        _wfc.SetCell(2, 7, HotDogStandCell, true);

        _wfc.SetCell(2, 8, BlankCell, true);
        _wfc.SetCell(2, 9, BlankCell, true);
        _wfc.SetCell(3, 8, BlankCell, true);
        _wfc.SetCell(4, 8, BlankCell, true);
        _wfc.SetCell(4, 7, BlankCell, true);
        _wfc.SetCell(4, 6, BlankCell, true);

        _wfc.SetCell(5, 6, BlankCell, true);
        _wfc.SetCell(6, 6, BlankCell, true);

        _wfc.SetCell(6, 5, BlankCell, true);
        _wfc.SetCell(6, 4, BlankCell, true);
        _wfc.SetCell(6, 7, BlankCell, true);
        _wfc.SetCell(6, 8, BlankCell, true);

        _wfc.SetCell(7, 6, CheeseStoreCell, true);
    }

    private List<WFCCell> Reduce(int row, int col, PendingCell<WFCCell> cell)
    {
        // First, check if any neighbors are parts of a bigger cell that would include this one, and narrow to that
        // type if needed. If that part isn't in our list of possibilities, that's a problem. Also remove any multi-cell
        // possibilities that wouldn't work next to current neighbors.

        // Then, reduce based on neighbor connections.
        //   If a neighbor _must_ connect, reduce to only connected types
        //   If a neighbor _musn't_ connect, reduce to only unconnected types
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
