using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameGenerator : MonoBehaviour
{
    public int Rows = 10;
    public int Cols = 10;
    public List<GameObject> GameCellPrefabs;
    public GameObject PendingCellPrefab;

    public bool EnableDebug = true;
    public bool GenerateImmediately = true;

    public GameObject MrDebugObject;

    public GameCellWFC WCF { get; private set; }
    public bool IsGenerationComplete { get; private set; } = false;

    private List<List<PendingCellGraphic>> _debugCells = new List<List<PendingCellGraphic>>();
    private List<List<GameObject>> _liveCells = new List<List<GameObject>>();

    void Start()
    {
        WCF = new GameCellWFC(Rows, Cols, GameCellPrefabs.Select(prefab => prefab.GetComponent<GameCell>()).ToList());

        if (EnableDebug)
        {
            ForEachPosition((row, col, location) =>
            {
                if (col == 0)
                {
                    _debugCells.Add(new List<PendingCellGraphic>());
                }

                var debugCell = Instantiate(PendingCellPrefab, location, transform.rotation);
                _debugCells[row].Add(debugCell.GetComponent<PendingCellGraphic>());
            });
        }

        if (GenerateImmediately)
        {
            GenerateComplete();
        }
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

    delegate void PositionVisiter(int row, int col, Vector3 location);
    private void ForEachPosition(PositionVisiter visiter)
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                var x = col * GameCell.CellWidth;
                var z = row * GameCell.CellWidth;

                visiter(row, col, new Vector3(x, 0f, z));
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
            MrDebugObject.GetComponentInChildren<MrDebug>().GameCells = _liveCells.SelectMany(row => row).Where(cell => cell != null).ToList();
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
        ForEachPosition((row, col, location) => _debugCells[row][col].SetOptions(grid.GetCell(row, col), WCF.IsCellQueued(row, col)));
    }

    private void SpawnCells()
    {
        var grid = WCF.GetGrid();

        ForEachPosition((row, col, location) =>
        {
            if (col == 0)
            {
                _liveCells.Add(new List<GameObject>());
            }


            var cell = grid.GetCell(row, col);

            if (cell.PossibleCells.Count != 1)
            {
                _liveCells[row].Add(null);
                return;
            }

            var cellDef = cell.PossibleCells[0];
            if (cellDef.InnerRow != 0 || cellDef.InnerCol != 0)
            {
                _liveCells[row].Add(null);
                return;
            }

            var cellObject = Instantiate(
                cellDef.BaseCell.gameObject,
                location,
                transform.rotation
            );

            _liveCells[row].Add(cellObject);
        });
    }

    private void ConnectCells()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                var cellObject = _liveCells[row][col];
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

                    point.toCell = _liveCells[neighborRow][neighborCol];
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
                var cellObject = _liveCells[row][col];
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
