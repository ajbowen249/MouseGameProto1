using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameGenerator : MonoBehaviour
{
    public int Rows = 10;
    public int Cols = 10;
    public List<GameObject> GameCellPrefabs;
    public GameObject MrDebugObject;

    private GameCellWFC _wcf;
    private List<List<GameObject>> _liveCells = new List<List<GameObject>>();

    void Start()
    {
        _wcf = new GameCellWFC(Rows, Cols, GameCellPrefabs.Select(prefab => prefab.GetComponent<GameCell>()).ToList());
        _wcf.Generate();
        SpawnCells();
        ConnectCells();
        NotifyGenerationComplete();

        var mrDebug = MrDebugObject?.GetComponentInChildren<MrDebug>();
        if (mrDebug != null)
        {
            mrDebug.GameCells = _liveCells.SelectMany(row => row).Where(cell => cell != null).ToList();
        }
    }

    private void SpawnCells()
    {
        var grid = _wcf.GetGrid();

        for (int row = 0; row < Rows; row++)
        {
            var cellRow = new List<GameObject>();
            for (int col = 0; col < Cols; col++)
            {
                var x = col * GameCell.CellWidth;
                var z = row * GameCell.CellWidth;

                var cell = grid.GetCell(row, col);
                if (cell.PossibleCells.Count != 1)
                {
                    cellRow.Add(null);
                    continue;
                }

                var cellDef = cell.PossibleCells[0];
                if (cellDef.InnerRow != 0 || cellDef.InnerCol != 0)
                {
                    cellRow.Add(null);
                    continue;
                }

                var cellObject = Instantiate(
                    cellDef.BaseCell.gameObject,
                    new Vector3(x, 0f, z),
                    transform.rotation
                );

                cellRow.Add(cellObject);
            }

            _liveCells.Add(cellRow);
        }
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
