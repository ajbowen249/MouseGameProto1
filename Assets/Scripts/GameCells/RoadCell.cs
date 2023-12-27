using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadCell : MonoBehaviour
{
    private const float SidewalkSize = 4;

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

    private List<(AbsoluteAttachPoint point, int row, int col)> _attachPoints;

    void OnDeterminedConnections(List<(AbsoluteAttachPoint point, int row, int col)> attachPoints)
    {
        _attachPoints = attachPoints;
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

    void OnGenerationComplete(GameCell cell)
    {
        GenerateSidewalk();
    }

    private void GenerateSidewalk()
    {
        var carPoints = _attachPoints.Where(point => point.point.modeType == AttachModeType.CAR);

        if (carPoints.Count() == 0)
        {
            return;
        }

        var carConnectsNorth = carPoints.Any(point => point.point.edge == AttachEdge.NORTH);
        var carConnectsSouth = carPoints.Any(point => point.point.edge == AttachEdge.SOUTH);
        var carConnectsEast = carPoints.Any(point => point.point.edge == AttachEdge.EAST);
        var carConnectsWest = carPoints.Any(point => point.point.edge == AttachEdge.WEST);

        float insetFromEdge = GameCell.HalfWidth - (SidewalkSize / 2f);

        Action<GameObject, Vector3, Quaternion> SpawnSegment = (prefab, offset, rotation) => {
            Instantiate(
                prefab,
                transform.position + new Vector3(0f, 0.03f, 0f) + offset,
                rotation,
                transform
            );
        };

        Action<Vector3, Quaternion> SpawnStraightSegment = (offset, rotation) => {
            SpawnSegment(StraightSidewalkPrefab, offset, rotation);
        };

        Action<Vector3, Quaternion> SpawnCornerSegment = (offset, rotation) => {
            SpawnSegment(CornerSidewalkPrefab, offset, rotation);
        };

        Action<int, Quaternion, Func<int, Vector3>> drawSidewalk = (segments, rotation, offsetGetter) => {
            for (int i = 0; i < segments; i++)
            {
                SpawnStraightSegment(offsetGetter(i), rotation);
            }
        };

        var northRotation = Quaternion.Euler(0f, 90f, 0f);

        if (!carConnectsNorth)
        {
            drawSidewalk(
                3,
                northRotation,
                i => new Vector3((i * SidewalkSize) - SidewalkSize, 0f, insetFromEdge)
            );

            if (carConnectsEast)
            {
                SpawnStraightSegment(new Vector3(insetFromEdge, 0f, insetFromEdge), northRotation);
            }

            if (carConnectsWest)
            {
                SpawnStraightSegment(new Vector3(-1f * insetFromEdge, 0f, insetFromEdge), northRotation);
            }
        }

        var southRotation = Quaternion.Euler(0f, -90f, 0f);

        if (!carConnectsSouth)
        {
            drawSidewalk(
                3,
                southRotation,
                i => new Vector3((i * SidewalkSize) - SidewalkSize, 0f, -1f * insetFromEdge)
            );

            if (carConnectsEast)
            {
                SpawnStraightSegment(new Vector3(insetFromEdge, 0f, -1 * insetFromEdge), southRotation);
            }

            if (carConnectsWest)
            {
                SpawnStraightSegment(new Vector3(-1f * insetFromEdge, 0f, -1 * insetFromEdge), southRotation);
            }
        }

        var eastRotation = Quaternion.Euler(0f, 180f, 0f);

        if (!carConnectsEast)
        {
            drawSidewalk(
                3,
                eastRotation,
                i => new Vector3(insetFromEdge, 0f, (i * SidewalkSize) - SidewalkSize)
            );

            if (carConnectsNorth)
            {
                SpawnStraightSegment(new Vector3(insetFromEdge, 0f, insetFromEdge), eastRotation);
            }

            if (carConnectsSouth)
            {
                SpawnStraightSegment(new Vector3(insetFromEdge, 0f, -1f * insetFromEdge), eastRotation);
            }
        }

        var westRotation = Quaternion.Euler(0f, 0f, 0f);

        if (!carConnectsWest)
        {
            drawSidewalk(
                3,
                westRotation,
                i => new Vector3(-1f * insetFromEdge, 0f, (i * SidewalkSize) - SidewalkSize)
            );

            if (carConnectsNorth)
            {
                SpawnStraightSegment(new Vector3(-1f * insetFromEdge, 0f, insetFromEdge), westRotation);
            }

            if (carConnectsSouth)
            {
                SpawnStraightSegment(new Vector3(-1f * insetFromEdge, 0f, -1f * insetFromEdge), westRotation);
            }
        }

        if (carConnectsNorth && carConnectsEast)
        {
            SpawnCornerSegment(new Vector3(insetFromEdge, 0f, insetFromEdge), northRotation);
        }

        if (carConnectsNorth && carConnectsWest)
        {
            SpawnCornerSegment(new Vector3(-1f * insetFromEdge, 0f, insetFromEdge), westRotation);
        }

        if (carConnectsSouth && carConnectsEast)
        {
            SpawnCornerSegment(new Vector3(insetFromEdge, 0f, -1f * insetFromEdge), eastRotation);
        }

        if (carConnectsSouth && carConnectsWest)
        {
            SpawnCornerSegment(new Vector3(-1f * insetFromEdge, 0f, -1f * insetFromEdge), southRotation);
        }
    }
}
