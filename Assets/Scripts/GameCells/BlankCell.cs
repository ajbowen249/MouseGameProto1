using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlankCell : MonoBehaviour
{
    public GameObject NorthRoad;
    public GameObject SouthRoad;
    public GameObject EastRoad;
    public GameObject WestRoad;

    public GameObject NorthExit;
    public GameObject SouthExit;
    public GameObject EastExit;
    public GameObject WestExit;

    void OnGenerationComplete(GameCell cell)
    {
        // Clear them all so we set active if at least one point is connected
        NorthRoad.SetActive(false);
        SouthRoad.SetActive(false);
        EastRoad.SetActive(false);
        WestRoad.SetActive(false);

        NorthExit.SetActive(false);
        SouthExit.SetActive(false);
        EastExit.SetActive(false);
        WestExit.SetActive(false);

        foreach (var point in cell.AllExitPoints.Where(point => point.toCell != null))
        {
            var toCell = point.toCell.GetComponent<GameCell>();
            var toPoint = toCell.EntryPoints.Find(toPoint =>
                point.mode.type == toPoint.mode.type &&
                point.edge == toPoint.edge.Opposite()
            );

            if (toPoint == null)
            {
                continue;
            }

            switch (point.mode.type)
            {
                case AttachModeType.CAR:
                    switch (point.edge)
                    {
                        case AttachEdge.NORTH:
                            NorthRoad.SetActive(true);
                            break;
                        case AttachEdge.SOUTH:
                            SouthRoad.SetActive(true);
                            break;
                        case AttachEdge.EAST:
                            EastRoad.SetActive(true);
                            break;
                        case AttachEdge.WEST:
                            WestRoad.SetActive(true);
                            break;
                    }
                    break;
                case AttachModeType.FOOT:
                    switch (point.edge)
                    {
                        case AttachEdge.NORTH:
                            NorthExit.SetActive(true);
                            break;
                        case AttachEdge.SOUTH:
                            SouthExit.SetActive(true);
                            break;
                        case AttachEdge.EAST:
                            EastExit.SetActive(true);
                            break;
                        case AttachEdge.WEST:
                            WestExit.SetActive(true);
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
