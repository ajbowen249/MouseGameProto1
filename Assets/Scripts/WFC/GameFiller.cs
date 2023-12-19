using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameFiller
{
    private WFCContext<WFCCell> _wfc;
    private List<WFCCell> _allCellTypes;
    private List<WFCCell> _allRoadCellTypes;

    private int _randomSeed;
    private System.Random _random;

    public int RandomSeed
    {
        get
        {
            return _randomSeed;
        }
        set
        {
            _randomSeed = value;
            _random = new System.Random(_randomSeed);
        }
    }

    public GameFiller(WFCContext<WFCCell> wfc, List<WFCCell> allCellTypes)
    {
        _wfc = wfc;
        _allCellTypes = allCellTypes;
        _allRoadCellTypes = _allCellTypes.Where(t => t.BaseCell.gameObject.name == "RoadCell").ToList();
    }

    public void PlaceFixedCells()
    {
        var HomeCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "HomeCell");
        var YardCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "YardCell");
        var BlockedRoadCell = RandomElement(_allCellTypes.Where(cell =>
            cell.BaseCell.gameObject.name == "BlockedRoadCell" && cell.InnerRow == 0 && cell.InnerCol == 0
        ).ToList());

        var HotDogStandCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "HotDogStandCell");
        var CheeseStoreCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "CheeseStoreCell");

        Func<List<AttachEdge>, bool, WFCCell> randomRoadCell = (edges, allowFoot) => RandomElement(_allRoadCellTypes.Where(
            t => edges.All(edge => t.AttachPoints.Any(point => (allowFoot || point.modeType == AttachModeType.CAR) && point.edge == edge)
        )).ToList());

        _wfc.SetCell(0, 5, HomeCell, true);
        _wfc.SetCell(1, 5, YardCell, true);
        _wfc.SetCell(2, 5, BlockedRoadCell, true);
        _wfc.SetCell(2, 7, HotDogStandCell, true);

        _wfc.SetCell(2, 8, randomRoadCell(new List<AttachEdge> { AttachEdge.WEST, AttachEdge.NORTH }, false), true);
        _wfc.SetCell(3, 8, randomRoadCell(new List<AttachEdge> { AttachEdge.NORTH, AttachEdge.SOUTH }, false), true);
        _wfc.SetCell(4, 8, randomRoadCell(new List<AttachEdge> { AttachEdge.WEST, AttachEdge.SOUTH }, false), true);
        _wfc.SetCell(4, 7, randomRoadCell(new List<AttachEdge> { AttachEdge.EAST, AttachEdge.WEST }, false), true);
        _wfc.SetCell(4, 6, randomRoadCell(new List<AttachEdge> { AttachEdge.EAST, AttachEdge.NORTH }, false), true);
        _wfc.SetCell(5, 6, randomRoadCell(new List<AttachEdge> { AttachEdge.NORTH, AttachEdge.SOUTH }, false), true);
        _wfc.SetCell(6, 6, randomRoadCell(new List<AttachEdge> { AttachEdge.NORTH, AttachEdge.SOUTH }, true), true);

        _wfc.SetCell(7, 6, CheeseStoreCell, true);
    }

    public void RandomFill()
    {
        // TODO: This should eventually have logic for things like sensible-looking roads and spawn rates for cells
        // For now, it's honest-to-goodness random

        var pickableCells = _wfc.Grid.GetCells()
            .SelectMany((row, rowIndex) => row.Select((cell, colIndex) => (cell, rowIndex, colIndex)))
            .Where(cell => cell.Item1.PossibleCells.Count > 1)
            .ToList();

        var randomCell = RandomElement(pickableCells);

        // Placeholder until more advanced logic is available. Pick from distinct types so connection variants don't
        // boost likelihood.
        var baseTypeOptions = randomCell.cell.PossibleCells.Select(point => point.BaseCell)
            .Distinct().ToList();

        var randomBase = RandomElement(baseTypeOptions);
        var randomType = RandomElement(randomCell.cell.PossibleCells.Where(cell => cell.BaseCell == randomBase).ToList());

        _wfc.SetCell(randomCell.rowIndex, randomCell.colIndex, randomType, false);
    }

    private T RandomElement<T>(IList<T> elements)
    {
        var randomIndex = _random.Next(elements.Count);
        return elements[randomIndex];
    }

    private void LayStrip(
    int startRow,
    int startCol,
    int length,
    int rowStep,
    int colStep,
    IEnumerable<AttachModeType> modes
)
    {
        var row = startRow;
        var col = startCol;
        var step = 0;

        var edge1 = rowStep != 0 ? (rowStep > 0 ? AttachEdge.SOUTH : AttachEdge.NORTH) :
            (colStep > 0 ? AttachEdge.WEST : AttachEdge.EAST);

        var edge2 = rowStep != 0 ? (rowStep > 0 ? AttachEdge.NORTH : AttachEdge.SOUTH) :
            (colStep > 0 ? AttachEdge.EAST : AttachEdge.WEST);

        var grid = _wfc.Grid;

        while (step < length)
        {
            var pendingCell = grid.GetCell(row, col);
            if (pendingCell == null)
            {
                return;
            }

            _wfc.SetCell(row, col, pendingCell.PossibleCells.Where(option =>
            {
                var points = option.AttachPoints.Where(point => modes.Contains(point.modeType)).ToList();
                var result = (step == 0 || points.Any(points => points.edge == edge1)) &&
                    (step == length - 1 || points.Any(point => point.edge == edge2));

                return result;
            }).ToList(), true);

            row += rowStep;
            col += colStep;
            step++;
        }
    }
}
