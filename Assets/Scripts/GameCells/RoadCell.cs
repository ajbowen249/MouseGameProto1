using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadCell : MonoBehaviour
{
    public GameObject StraightSidewalkPrefab;
    public GameObject CornerSidewalkPrefab;

    public GameObject NorthRoad;
    public GameObject SouthRoad;
    public GameObject EastRoad;
    public GameObject WestRoad;

    public GameObject NorthExit;
    public GameObject SouthExit;
    public GameObject EastExit;
    public GameObject WestExit;

    void OnDeterminedConnections(List<(AbsoluteAttachPoint point, int row, int col)> attachPoints)
    {
        var gameCell = GetComponent<GameCell>();

        // Clear them all so we set active if at least one point is connected
        NorthRoad.SetActive(false);
        SouthRoad.SetActive(false);
        EastRoad.SetActive(false);
        WestRoad.SetActive(false);

        NorthExit.SetActive(false);
        SouthExit.SetActive(false);
        EastExit.SetActive(false);
        WestExit.SetActive(false);

        foreach (var point in attachPoints)
        {
            switch (point.point.modeType)
            {
                case AttachModeType.CAR:
                    switch (point.point.edge)
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
                    switch (point.point.edge)
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
