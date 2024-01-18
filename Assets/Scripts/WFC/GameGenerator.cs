using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameGenerator : MonoBehaviour
{
    public static GameGenerator Instance;

    public int Rows = 20;
    public int Cols = 10;
    public List<GameObject> GameCellPrefabs;
    public GameObject PendingCellPrefab;

    public bool EnableDebug = true;
    public bool GenerateImmediately = true;

    public GameObject MrDebugObject;

    public GameCellWFC WCF { get; private set; }
    public bool IsGenerationComplete { get; private set; } = false;
    public List<List<GameObject>> GameCellGrid { get; private set; } = new List<List<GameObject>>();

    private List<List<PendingCellGraphic>> _debugCells = new List<List<PendingCellGraphic>>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        Init();
    }

    public void GenerateComplete()
    {
        WCF.GenerateComplete();
        OnWCFComplete();
    }

    public void Step()
    {
        if (IsGenerationComplete)
        {
            return;
        }

        if (WCF.CanIterate)
        {
            WCF.Iterate();
            UpdateDebugCells();
            return;
        }

        OnWCFComplete();
        UpdateDebugCells();
    }

    public void ResetSelf()
    {
        foreach (var row in GameCellGrid)
        {
            foreach (var cell in row)
            {
                Destroy(cell);
            }
        }

        GameCellGrid = new List<List<GameObject>>();

        foreach (var row in _debugCells)
        {
            foreach (var cell in row)
            {
                Destroy(cell.gameObject);
            }
        }

        IsGenerationComplete = false;

        _debugCells = new List<List<PendingCellGraphic>>();
        Init();
    }

    public GameObject GetGameCell(GridLocation loc)
    {
        if (loc.row < 0 || loc.col < 0 || loc.row > Rows - 1 || loc.col > Cols - 1)
        {
            return null;
        }

        return GameCellGrid[loc.row][loc.col];
    }    

    private void Init()
    {
        RandomInstances.SetSeed(RandomInstances.Names.Generator, 0);
        WCF = new GameCellWFC(Rows, Cols, GameCellPrefabs.Select(prefab => prefab.GetComponent<GameCell>()).ToList());

        if (EnableDebug)
        {
            ForEachPosition((loc, location) =>
            {
                if (loc.col == 0)
                {
                    _debugCells.Add(new List<PendingCellGraphic>());
                }

                var debugCell = Instantiate(PendingCellPrefab, location, transform.rotation);
                _debugCells[loc.row].Add(debugCell.GetComponent<PendingCellGraphic>());
            });
        }

        if (GenerateImmediately)
        {
            GenerateComplete();
        }
    }

    delegate void PositionVisiter(GridLocation loc, Vector3 location);
    private void ForEachPosition(PositionVisiter visiter)
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                var x = col * GameCell.CellWidth;
                var z = row * GameCell.CellWidth;

                visiter((row, col), new Vector3(x, 0f, z));
            }
        }
    }

    private void OnWCFComplete()
    {
        SpawnCells();
        ConnectCells();
        UpdateDebugCells();
        NotifyGenerationComplete();

        if (EnableDebug && MrDebugObject != null)
        {
            MrDebugObject.GetComponentInChildren<MrDebug>().GameCells = GameCellGrid.SelectMany(row => row).Where(cell => cell != null).ToList();
        }

        IsGenerationComplete = true;
    }

    private void UpdateDebugCells()
    {
        if (!EnableDebug)
        {
            return;
        }

        var grid = WCF.GetGrid();
        ForEachPosition((loc, location) => _debugCells[loc.row][loc.col].SetOptions(loc, grid.GetCell(loc), WCF.IsCellQueued(loc)));
    }

    private void SpawnCells()
    {
        var grid = WCF.GetGrid();

        ForEachPosition((loc, location) =>
        {
            if (loc.col == 0)
            {
                GameCellGrid.Add(new List<GameObject>());
            }


            var cell = grid.GetCell(loc);

            if (cell.PossibleCells.Count != 1)
            {
                GameCellGrid[loc.row].Add(null);
                return;
            }

            var cellDef = cell.PossibleCells[0];
            if (cellDef.InnerRow != 0 || cellDef.InnerCol != 0)
            {
                GameCellGrid[loc.row].Add(null);
                return;
            }

            var cellObject = Instantiate(
                cellDef.BaseCell.gameObject,
                location,
                transform.rotation
            );

            var connections = cellDef.AttachPoints.Select(point => (point, new GridLocation(0, 0))).ToList();

            if (cellDef.IsSubCell)
            {
                foreach (var additionalCell in cellDef.BaseCell.Footprint)
                {
                    if (additionalCell.row == 0 && additionalCell.col == 0)
                    {
                        continue;
                    }

                    var subLoc = (loc.row + additionalCell.row, loc.col + additionalCell.col);

                    WFCCell subCell = null;
                    var subGridCell = grid.GetCell(subLoc);
                    if (subGridCell != null)
                    {
                        subCell = subGridCell.PossibleCells.Count > 0 ? subGridCell.PossibleCells.First() : null;
                    }

                    if (subCell == null || subCell.BaseCell != cellDef.BaseCell)
                    {
                        Debug.LogError($"Expected {subLoc} to share base with {loc} ({cellDef.BaseCell.name})");
                    }

                    connections.AddRange(subCell.AttachPoints.Select(point => (point, new GridLocation(additionalCell.row, additionalCell.col))));
                }
            }

            var gameCell = cellObject.GetComponent<GameCell>();
            gameCell.Row = loc.row;
            gameCell.Col = loc.col;
            gameCell.DeterminedConnections(connections);

            GameCellGrid[loc.row].Add(cellObject);
        });
    }

    private void ConnectCells()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                var cellObject = GameCellGrid[row][col];
                if (cellObject == null)
                {
                    continue;
                }

                var cell = cellObject.GetComponent<GameCell>();
                var exits = cellObject.GetComponentsInChildren<Exit>().ToList();

                foreach (var point in cell.AllAttachPoints)
                {
                    var neighborRow = row + point.row;
                    var neighborCol = col + point.col;

                    switch (point.edge)
                    {
                        case AttachEdge.NORTH:
                            neighborRow++;
                            break;
                        case AttachEdge.SOUTH:
                            neighborRow--;
                            break;
                        case AttachEdge.EAST:
                            neighborCol++;
                            break;
                        case AttachEdge.WEST:
                            neighborCol--;
                            break;
                        default:
                            break;
                    }

                    if (neighborRow < 0 || neighborCol < 0 || neighborRow >= Rows || neighborCol >= Cols)
                    {
                        continue;
                    }

                    point.toCell = GameCellGrid[neighborRow][neighborCol];
                }
            }
        }
    }
    private void NotifyGenerationComplete()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                var cellObject = GameCellGrid[row][col];
                var cell = cellObject?.GetComponent<GameCell>();
                if (cell == null)
                {
                    continue;
                }

                cell.GenerationComplete();
            }
        }
    }
}
