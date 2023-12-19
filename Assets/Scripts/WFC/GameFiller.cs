using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameFiller
{
    private WFCContext<WFCCell> _wfc;
    private List<WFCCell> _allCellTypes;
    private Dictionary<Biome, List<WFCCell>> _cellsByBiome;

    // Road as in "RoadCell," not the Road biome!
    private List<WFCCell> _allRoadCellTypes;

    SeededRandom _random;

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
        DefineRoads(residentialHeight);

        var HomeCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "HomeCell");
        var YardCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "YardCell");
        var BlockedRoadCell = RandomUtil.RandomElement(_allCellTypes.Where(cell =>
            cell.BaseCell.gameObject.name == "BlockedRoadCell" && cell.InnerRow == 0 && cell.InnerCol == 0
        ).ToList(), _random.Random);

        var HotDogStandCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "HotDogStandCell");
        var CheeseStoreCell = _allCellTypes.Find(cell => cell.BaseCell.gameObject.name == "CheeseStoreCell");

        _wfc.SetCell(0, 4, HomeCell, true);
        _wfc.SetCell(1, 4, YardCell, true);
        _wfc.SetCell(2, 4, BlockedRoadCell, true);
        _wfc.SetCell(2, 6, HotDogStandCell, true);

        // Temporary; Stick the cheese shop on the top row guaranteed earlier
        _wfc.SetCell(_wfc.Grid.Rows - 1, _wfc.Grid.Cols / 2, CheeseStoreCell, true);
    }

    public void RandomFill()
    {
        // TODO: This should eventually have logic for things like sensible-looking roads and spawn rates for cells
        // For now, it's honest-to-goodness random

        var pickableCells = _wfc.Grid.GetCells()
            .SelectMany((row, rowIndex) => row.Select((cell, colIndex) => (cell, rowIndex, colIndex)))
            .Where(cell => cell.Item1.PossibleCells.Count > 1)
            .ToList();

        var randomCell = RandomUtil.RandomElement(pickableCells, _random.Random);

        // Placeholder until more advanced logic is available. Pick from distinct types so connection variants don't
        // boost likelihood.
        var baseTypeOptions = randomCell.cell.PossibleCells.Select(point => point.BaseCell)
            .Distinct().ToList();

        var randomBase = RandomUtil.RandomElement(baseTypeOptions, _random.Random);
        var randomType = RandomUtil.RandomElement(
            randomCell.cell.PossibleCells.Where(cell => cell.BaseCell == randomBase).ToList(),
            _random.Random
        );

        _wfc.SetCell(randomCell.rowIndex, randomCell.colIndex, randomType, false);
    }

    private void DefineBiomes(int residentialHeight)
    {
        // This should eventually be more random. Just testing stuff for now.

        // First phase is large and doesn't exclude roads, since some roads are exclusive to certain biomes.
        // Second phase excludes roads to define actual "blocks."

        var urbanList = new List<Biome> { Biome.URBAN };
        var suburbanList = new List<Biome> { Biome.SUBURBAN};
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

    private void DefineRoads(int residentialHeight)
    {
        // Ensure a southern "home street"
        // Using road specifically instead of biomes so ensure any point is walkable
        var emptyTypeList = new List<WFCCell>();
        var carMode = new List<AttachModeType> { AttachModeType.CAR };

        DefineTypeBlock(
            residentialHeight - 1,
            1,
            1,
            _wfc.Grid.Cols - 3,
            _allRoadCellTypes,
            emptyTypeList
        );

        ConnectStrip(
            residentialHeight - 1,
            1,
            _wfc.Grid.Cols - 3,
            0,
            1,
            carMode
        );

        // Ensure a southern road. The target shop can at least go here
        DefineTypeBlock(
            _wfc.Grid.Rows - 2,
            1,
            1,
            _wfc.Grid.Cols - 2,
            _allRoadCellTypes,
            emptyTypeList
        );

        ConnectStrip(
            _wfc.Grid.Rows - 2,
            1,
            _wfc.Grid.Cols - 3,
            0,
            1,
            carMode
        );

        // Temporary; Make a basic road connecting the eastern edges

        DefineTypeBlock(
            residentialHeight - 1,
            _wfc.Grid.Cols - 3,
            _wfc.Grid.Rows - 3,
            1,
            _allRoadCellTypes,
            emptyTypeList
        );

        ConnectStrip(
            residentialHeight - 1,
            _wfc.Grid.Cols - 3,
            _wfc.Grid.Rows - 3,
            1,
            0,
            carMode
        );
    }

    private void DefineBiomeBlock(int startRow, int startCol, int height, int width, List<Biome> includeBiomes, List<Biome> excludeBiomes)
    {
        NarrowBlockByCondition(startRow, startCol, height, width,
            option => (includeBiomes.Count == 0 || option.BaseCell.Biomes.Any(biome => includeBiomes.Contains(biome))) &&
                        !option.BaseCell.Biomes.Any(biome => excludeBiomes.Contains(biome))
        );
    }

    private void DefineTypeBlock(int startRow, int startCol, int height, int width, List<WFCCell> includeTypes, List<WFCCell> excludeTypes)
    {
        NarrowBlockByCondition(startRow, startCol, height, width,
            option => (includeTypes.Count == 0 || includeTypes.Contains(option)) && !excludeTypes.Contains(option)
        );
    }

    private void NarrowBlockByCondition(int startRow, int startCol, int height, int width, Func<WFCCell, bool> condition)
    {
        for (var row = startRow; row < startRow + height; row++)
        {
            for (var col = startCol; col < startCol + width; col++)
            {
                var cell = _wfc.Grid.GetCell(row, col);
                if (cell == null)
                {
                    Debug.LogWarning($"Tried to get cell {row}, {col}");
                    continue;
                }

                var narrowedOptions = cell.PossibleCells.Where(condition).ToList();

                if (narrowedOptions.Count > 0)
                {
                    _wfc.SetCell(row, col, narrowedOptions, true);
                }
            }
        }
    }

    private void ConnectStrip(
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
