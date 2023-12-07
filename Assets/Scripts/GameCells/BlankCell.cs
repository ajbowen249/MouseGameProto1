using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlankCell : MonoBehaviour
{
    public GameObject NorthRoad;
    public GameObject SouthRoad;
    public GameObject EastRoad;
    public GameObject WestRoad;

    void OnGenerationComplete(GameCell cell)
    {
        // Clear them all so we set active if at least one point is connected
        NorthRoad.SetActive(false);
        SouthRoad.SetActive(false);
        EastRoad.SetActive(false);
        WestRoad.SetActive(false);

        foreach (var point in cell.AllAttachPoints)
        {
            if (point.toCell == null)
            {
                continue;
            }

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
        }
    }
}
