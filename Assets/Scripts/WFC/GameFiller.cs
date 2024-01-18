using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;

public class GameFiller
{
    private WFCContext<WFCCell> _wfc;
    private List<WFCCell> _allCellTypes;
    private Dictionary<Biome, List<WFCCell>> _cellsByBiome;

    // Road as in "RoadCell," not the Road biome!
    private List<WFCCell> _allRoadCellTypes;

    SeededRandom _random;

    private List<WFCCell> _emptyTypeList = new List<WFCCell>();
    private List<AttachModeType> _carMode = new List<AttachModeType> { AttachModeType.CAR };

    private GridLocation? _randomPathStart;
    private GridLocation? _randomPathLast;
    private GridLocation? _randomPathCurrent;

    public bool HasPlacedFixedCells
    {
        get { return _randomPathStart != null; }
    }

    public List<GridLocation> GeneratedPath { get; private set; } = new List<GridLocation>();

    public bool HasCreatedRandomPath { get; private set; } = false;

    public GameFiller(WFCContext<WFCCell> wfc, List<WFCCell> allCellTypes)
    {
        _random = RandomInstances.GetInstance(RandomInstances.Names.Generator);
        _wfc = wfc;
        _allCellTypes = allCellTypes;
        _allRoadCellTypes = _allCellTypes.Where(t => t.BaseCell.gameObject.name == "RoadCell").ToList();
        _cellsByBiome = new Dictionary<Biome, List<WFCCell>>();
        foreach (var cell in _allRoadCellTypes)
        {
            foreach (var biome in cell.BaseCell.Biomes)
            {
                if (!_cellsByBiome.ContainsKey(biome))
                {
                    _cellsByBiome[biome] = new List<WFCCell>();
                }

                _cellsByBiome[biome].Add(cell);
            }
        }
    }

    public void PlaceFixedCells()
    {
        var residentialHeight = 3;

        DefineBiomes(residentialHeight);
        // The first cell of the auto path is the one to the east of the last "tutorial" encounter.
        GeneratedPath.Add((2, 7));
        _randomPathStart = AddFixedRoads(residentialHeight);

        var HomeCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "HomeCell");
        var YardCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "YardCell");
        var BlockedRoadCell = RandomUtil.RandomElement(_allCellTypes.Where(cell =>
            cell.BaseCell.gameObject.name == "BlockedRoadCell" && cell.InnerRow == 0 && cell.InnerCol == 0
        ).ToList(), _random);

        var HotDogStandCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "HotDogStandCell");
        var CheeseStoreCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "CheeseStoreCell");

        _wfc.SetCell((0, 4), HomeCell, true);
        _wfc.SetCell((1, 4), YardCell, true);
        _wfc.SetCell((2, 4), BlockedRoadCell, true);
        _wfc.SetCell((2, 6), HotDogStandCell, true);

        // Temporary; Stick the cheese shop on the top row guaranteed earlier
        _wfc.SetCell((_wfc.Grid.Rows - 1, _wfc.Grid.Cols / 2), CheeseStoreCell, true);
    }

    public void RandomFill()
    {
        // TODO: This should eventually have logic for things like sensible-looking roads and spawn rates for cells
        // For now, it's honest-to-goodness random

        var pickableCells = _wfc.Grid.GetCells()
            .SelectMany((row, rowIndex) => row.Select((cell, colIndex) => (cell, rowIndex, colIndex)))
            .Where(cell => cell.Item1.PossibleCells.Count > 1)
            .ToList();

        var randomCell = RandomUtil.RandomElement(pickableCells, _random);

        // Placeholder until more advanced logic is available. Pick from distinct types so connection variants don't
        // boost likelihood.
        var baseTypeOptions = randomCell.cell.PossibleCells.Select(point => point.BaseCell)
            .Distinct().ToList();

        // Try not to pick roads at random, if we can help it.
        var nonRoadOptions = baseTypeOptions.Where((cell) => cell.name != "RoadCell").ToList();
        if (nonRoadOptions.Count > 0)
        {
            baseTypeOptions = nonRoadOptions;
        }

        var randomBase = RandomUtil.RandomElement(baseTypeOptions, _random);
        var randomType = RandomUtil.RandomElement(
            randomCell.cell.PossibleCells.Where(cell => cell.BaseCell == randomBase).ToList(),
            _random
        );

        _wfc.SetCell((randomCell.rowIndex, randomCell.colIndex), randomType, false);
    }

    public void GeneratePathComplete()
    {
        while (!HasCreatedRandomPath)
        {
            IteratePathGeneration();
        }
    }

    public void IteratePathGeneration()
    {
        if (_randomPathStart == null)
        {
            throw new Exception("Tried laying path before fixed cells");
        }

        if (_randomPathCurrent == null)
        {
            _randomPathCurrent = _randomPathStart;
        }

        var currentLoc = (GridLocation)_randomPathCurrent;
        var startLocation = (GridLocation)_randomPathStart;

        if (currentLoc.row >= _wfc.Grid.Rows - 2)
        {
            HasCreatedRandomPath = true;
            return;
        }

        var nextLocationOptions = currentLoc.GetAllNeighborLocations()
            .Where(pair => pair.edge != AttachEdge.SOUTH)
            .Where(pair =>
                (_randomPathLast == null || pair.loc != (GridLocation)_randomPathLast) &&
            pair.loc.row >= startLocation.row &&
                pair.loc.col >= 1 &&
                pair.loc.col <= startLocation.col
            ).ToList();

        if (nextLocationOptions.Count == 0)
        {
            throw new Exception($"No available options when making path at {currentLoc}");
        }

        var nextLocation = RandomUtil.RandomElement(nextLocationOptions, _random).loc;
        GeneratedPath.Add(nextLocation);

        DefineTypeStrip(
            currentLoc.row,
            currentLoc.col,
            2,
            nextLocation.row - currentLoc.row,
            nextLocation.col - currentLoc.col,
            _allRoadCellTypes,
            _emptyTypeList
        );

        ConnectStrip(
            currentLoc.row,
            currentLoc.col,
            2,
            nextLocation.row - currentLoc.row,
            nextLocation.col - currentLoc.col,
            _carMode
        );

        _randomPathLast = _randomPathCurrent;
        _randomPathCurrent = nextLocation;
    }

    private void DefineBiomes(int residentialHeight)
    {
        // This should eventually be more random. Just testing stuff for now.

        // First phase is large and doesn't exclude roads, since some roads are exclusive to certain biomes.
        // Second phase excludes roads to define actual "blocks."

        var urbanList = new List<Biome> { Biome.URBAN };
        var suburbanList = new List<Biome> { Biome.SUBURBAN };
        var roadList = new List<Biome> { Biome.ROAD };
        var emptyList = new List<Biome> { };

        // Residential at the bottom
        DefineBiomeBlock(
            0,
            0,
            residentialHeight,
            _wfc.Grid.Cols,
            suburbanList,
            emptyList
        );

        // Set the rest to urban
        DefineBiomeBlock(
            residentialHeight,
            0,
            _wfc.Grid.Rows - residentialHeight,
            _wfc.Grid.Cols,
            urbanList,
            emptyList
        );

        // Drop roads along the edges
        // NO WAY IN. NO WAY OUT.
        DefineBiomeBlock(
            0,
            0,
            _wfc.Grid.Rows,
            1,
            emptyList,
            roadList
        );

        DefineBiomeBlock(
            0,
            _wfc.Grid.Cols - 1,
            _wfc.Grid.Rows,
            1,
            emptyList,
            roadList
        );

        DefineBiomeBlock(
            0,
            0,
            1,
            _wfc.Grid.Cols,
            emptyList,
            roadList
        );

        DefineBiomeBlock(
            _wfc.Grid.Rows - 1,
            0,
            1,
            _wfc.Grid.Cols,
            emptyList,
            roadList
        );
    }

    private GridLocation AddFixedRoads(int residentialHeight)
    {
        // Create a southern "home street"
        DefineTypeBlock(
            residentialHeight - 1,
            1,
            1,
            _wfc.Grid.Cols - 2,
            _allRoadCellTypes,
            _emptyTypeList
        );

        ConnectStrip(
            residentialHeight - 1,
            1,
            _wfc.Grid.Cols - 2,
            0,
            1,
            _carMode
        );

        // Create a northern road for the target shop
        DefineTypeBlock(
            _wfc.Grid.Rows - 2,
            1,
            1,
            _wfc.Grid.Cols - 2,
            _allRoadCellTypes,
            _emptyTypeList
        );

        ConnectStrip(
            _wfc.Grid.Rows - 2,
            1,
            _wfc.Grid.Cols - 2,
            0,
            1,
            _carMode
        );

        // At the end of the home road, turn north for a little bit.
        // This is the point from which we will start our meandering paths.

        DefineTypeBlock(
            residentialHeight - 1,
            _wfc.Grid.Cols - 2,
            3,
            1,
            _allRoadCellTypes,
            _emptyTypeList
        );

        GeneratedPath.AddRange(ConnectStrip(
            residentialHeight - 1,
            _wfc.Grid.Cols - 2,
            3,
            1,
            0,
            _carMode
        ));

        // Ensure there are no roads one row north of the home road, since that would look weird and we won't drive
        // there anyway.
        DefineTypeBlock(
            residentialHeight,
            2,
            1,
            _wfc.Grid.Cols - 4,
            _allCellTypes.Where(cell => cell.AttachPoints.Count == 0).ToList(),
            _emptyTypeList
        );

        return (residentialHeight + 1, _wfc.Grid.Cols - 2);
    }

    private List<GridLocation> DefineBiomeBlock(int startRow, int startCol, int height, int width, List<Biome> includeBiomes, List<Biome> excludeBiomes)
    {
        return NarrowBlockByCondition(startRow, startCol, height, width,
            option => (includeBiomes.Count == 0 || option.BaseCell.Biomes.Any(biome => includeBiomes.Contains(biome))) &&
                        !option.BaseCell.Biomes.Any(biome => excludeBiomes.Contains(biome))
        );
    }

    private List<GridLocation> DefineTypeBlock(int startRow, int startCol, int height, int width, List<WFCCell> includeTypes, List<WFCCell> excludeTypes)
    {
        return NarrowBlockByCondition(startRow, startCol, height, width,
            option => (includeTypes.Count == 0 || includeTypes.Contains(option)) && !excludeTypes.Contains(option)
        );
    }

    private List<GridLocation> NarrowBlockByCondition(int startRow, int startCol, int height, int width, Func<WFCCell, bool> condition)
    {
        var visitedLocations = new List<GridLocation>();

        for (var row = startRow; row < startRow + height; row++)
        {
            for (var col = startCol; col < startCol + width; col++)
            {
                GridLocation location = (row, col);
                var cell = _wfc.Grid.GetCell(location);
                if (cell == null)
                {
                    Debug.LogWarning($"Tried to get cell {row}, {col}");
                    continue;
                }

                visitedLocations.Add(location);

                var narrowedOptions = cell.PossibleCells.Where(condition).ToList();

                if (narrowedOptions.Count > 0)
                {
                    _wfc.SetCell((row, col), narrowedOptions, true);
                }
            }
        }

        return visitedLocations;
    }

    private List<GridLocation> DefineTypeStrip(
        int startRow,
        int startCol,
        int length,
        int rowStep,
        int colStep,
        List<WFCCell> includeTypes,
        List<WFCCell> excludeTypes
    )
    {
        return NarrowStripByCondition(startRow, startCol, length, rowStep, colStep, (option, step) =>
            (includeTypes.Count == 0 || includeTypes.Contains(option)) && !excludeTypes.Contains(option)
        );
    }

    private List<GridLocation> ConnectStrip(
        int startRow,
        int startCol,
        int length,
        int rowStep,
        int colStep,
        IEnumerable<AttachModeType> modes
    )
    {
        var edge1 = rowStep != 0 ? (rowStep > 0 ? AttachEdge.SOUTH : AttachEdge.NORTH) :
            (colStep > 0 ? AttachEdge.WEST : AttachEdge.EAST);

        var edge2 = rowStep != 0 ? (rowStep > 0 ? AttachEdge.NORTH : AttachEdge.SOUTH) :
            (colStep > 0 ? AttachEdge.EAST : AttachEdge.WEST);

        return NarrowStripByCondition(startRow, startCol, length, rowStep, colStep, (option, step) =>
        {
            var points = option.AttachPoints.Where(point => modes.Contains(point.modeType)).ToList();
            var result = (step == 0 || points.Any(points => points.edge == edge1)) &&
                (step == length - 1 || points.Any(point => point.edge == edge2));

            return result;
        });
    }

    private List<GridLocation> NarrowStripByCondition(
        int startRow,
        int startCol,
        int length,
        int rowStep,
        int colStep,
        Func<WFCCell, int, bool> condition
    )
    {
        var visitedLocations = new List<GridLocation>();

        var row = startRow;
        var col = startCol;
        var step = 0;

        var grid = _wfc.Grid;

        while (step < length)
        {
            var location = (row, col);
            var pendingCell = grid.GetCell(location);
            if (pendingCell == null)
            {
                return visitedLocations;
            }

            visitedLocations.Add(location);

            _wfc.SetCell(location, pendingCell.PossibleCells.Where((option) =>
                condition(option, step)
            ).ToList(), true);

            row += rowStep;
            col += colStep;
            step++;
        }

        return visitedLocations;
    }
}
