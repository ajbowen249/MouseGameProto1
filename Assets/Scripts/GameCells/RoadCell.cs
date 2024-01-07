using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class RoadCell : MonoBehaviour
{
    private const float SidewalkSize = 4;

    public GameObject StraightSidewalkPrefab;
    public GameObject CornerSidewalkPrefab;
    public GameObject InnerCornerSidewalkPrefab;

    public GameObject StraightRoad;
    public GameObject TurnRoad;
    public GameObject Intersection3Way;
    public GameObject Intersection4Way;
    public GameObject UnpaintedRoad;

    public GameObject NorthExit;
    public GameObject SouthExit;
    public GameObject EastExit;
    public GameObject WestExit;

    private List<(AbsoluteAttachPoint point, int row, int col)> _attachPoints;

    void OnDeterminedConnections(List<(AbsoluteAttachPoint point, int row, int col)> attachPoints)
    {
        _attachPoints = attachPoints;
        var gameCell = GetComponent<GameCell>();

        NorthExit.SetActive(false);
        SouthExit.SetActive(false);
        EastExit.SetActive(false);
        WestExit.SetActive(false);

        foreach (var footExit in attachPoints.Where(point => point.point.modeType == AttachModeType.FOOT))
        {
            switch (footExit.point.edge)
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
        }

        var carExits = attachPoints.Where(point => point.point.modeType == AttachModeType.CAR)
            .Select(point => point.point.edge).Distinct().ToHashSet();

        switch (carExits.Count)
        {
            case 2:
                if (carExits.Contains(AttachEdge.EAST) && carExits.Contains(AttachEdge.WEST))
                {
                    StraightRoad.SetActive(true);
                }
                else if (carExits.Contains(AttachEdge.NORTH) && carExits.Contains(AttachEdge.SOUTH))
                {
                    StraightRoad.SetActive(true);
                    var rotation = StraightRoad.transform.rotation;
                    StraightRoad.transform.rotation = Quaternion.Euler(rotation.eulerAngles + new Vector3(0, 90, 0));
                }
                else if (carExits.Contains(AttachEdge.SOUTH) && carExits.Contains(AttachEdge.EAST))
                {
                    TurnRoad.SetActive(true);
                }
                else if (carExits.Contains(AttachEdge.SOUTH) && carExits.Contains(AttachEdge.WEST))
                {
                    TurnRoad.SetActive(true);
                    var rotation = TurnRoad.transform.rotation;
                    TurnRoad.transform.rotation = Quaternion.Euler(rotation.eulerAngles + new Vector3(0, 90, 0));
                }
                else if (carExits.Contains(AttachEdge.WEST) && carExits.Contains(AttachEdge.NORTH))
                {
                    TurnRoad.SetActive(true);
                    var rotation = TurnRoad.transform.rotation;
                    TurnRoad.transform.rotation = Quaternion.Euler(rotation.eulerAngles + new Vector3(0, 180, 0));
                }
                else if (carExits.Contains(AttachEdge.EAST) && carExits.Contains(AttachEdge.NORTH))
                {
                    TurnRoad.SetActive(true);
                    var rotation = TurnRoad.transform.rotation;
                    TurnRoad.transform.rotation = Quaternion.Euler(rotation.eulerAngles + new Vector3(0, 270, 0));
                }

                break;
            case 3:
                {
                    Intersection3Way.SetActive(true);
                    var rotation = Intersection3Way.transform.rotation;

                    if (carExits.SetEquals(new HashSet<AttachEdge> { AttachEdge.NORTH, AttachEdge.EAST, AttachEdge.SOUTH }))
                    {
                        Intersection3Way.transform.rotation = Quaternion.Euler(rotation.eulerAngles + new Vector3(0, 90, 0));
                    }
                    else if (carExits.SetEquals(new HashSet<AttachEdge> { AttachEdge.EAST, AttachEdge.SOUTH, AttachEdge.WEST }))
                    {
                        Intersection3Way.transform.rotation = Quaternion.Euler(rotation.eulerAngles + new Vector3(0, 180, 0));
                    }
                    else if (carExits.SetEquals(new HashSet<AttachEdge> { AttachEdge.SOUTH, AttachEdge.WEST, AttachEdge.NORTH }))
                    {
                        Intersection3Way.transform.rotation = Quaternion.Euler(rotation.eulerAngles + new Vector3(0, 270, 0));
                    }
                    break;
                }
            case 4:
                Intersection4Way.SetActive(true);
                break;
            default:
                UnpaintedRoad.SetActive(true);
                break;
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

        Action<GameObject, Vector3, Quaternion> SpawnSegment = (prefab, offset, rotation) =>
        {
            Instantiate(
                prefab,
                transform.position + new Vector3(0f, 0.03f, 0f) + offset,
                rotation,
                transform
            );
        };

        Action<Vector3, Quaternion> AddStraight = (offset, rotation) =>
        {
            SpawnSegment(StraightSidewalkPrefab, offset, rotation);
        };

        Action<Vector3, Quaternion> AddCorner = (offset, rotation) =>
        {
            SpawnSegment(CornerSidewalkPrefab, offset, rotation);
        };

        Action<Vector3, Quaternion> AddInnerCorner = (offset, rotation) =>
        {
            SpawnSegment(InnerCornerSidewalkPrefab, offset, rotation);
        };

        Action<int, Quaternion, Func<int, Vector3>> drawSidewalk = (segments, rotation, offsetGetter) =>
        {
            for (int i = 0; i < segments; i++)
            {
                AddStraight(offsetGetter(i), rotation);
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
                AddStraight(new Vector3(insetFromEdge, 0f, insetFromEdge), northRotation);
            }

            if (carConnectsWest)
            {
                AddStraight(new Vector3(-1f * insetFromEdge, 0f, insetFromEdge), northRotation);
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
                AddStraight(new Vector3(insetFromEdge, 0f, -1 * insetFromEdge), southRotation);
            }

            if (carConnectsWest)
            {
                AddStraight(new Vector3(-1f * insetFromEdge, 0f, -1 * insetFromEdge), southRotation);
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
                AddStraight(new Vector3(insetFromEdge, 0f, insetFromEdge), eastRotation);
            }

            if (carConnectsSouth)
            {
                AddStraight(new Vector3(insetFromEdge, 0f, -1f * insetFromEdge), eastRotation);
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
                AddStraight(new Vector3(-1f * insetFromEdge, 0f, insetFromEdge), westRotation);
            }

            if (carConnectsSouth)
            {
                AddStraight(new Vector3(-1f * insetFromEdge, 0f, -1f * insetFromEdge), westRotation);
            }
        }

        if (carConnectsNorth && carConnectsEast)
        {
            AddCorner(new Vector3(insetFromEdge, 0f, insetFromEdge), northRotation);
        }
        else if (!carConnectsNorth && !carConnectsEast)
        {
            AddInnerCorner(new Vector3(insetFromEdge, 0f, insetFromEdge), eastRotation);
        }

        if (carConnectsNorth && carConnectsWest)
        {
            AddCorner(new Vector3(-1f * insetFromEdge, 0f, insetFromEdge), westRotation);
        }
        else if (!carConnectsNorth && !carConnectsWest)
        {
            AddInnerCorner(new Vector3(-1f * insetFromEdge, 0f, insetFromEdge), northRotation);
        }

        if (carConnectsSouth && carConnectsEast)
        {
            AddCorner(new Vector3(insetFromEdge, 0f, -1f * insetFromEdge), eastRotation);
        }
        else if (!carConnectsSouth && !carConnectsEast)
        {
            AddInnerCorner(new Vector3(insetFromEdge, 0f, -1f * insetFromEdge), southRotation);
        }

        if (carConnectsSouth && carConnectsWest)
        {
            AddCorner(new Vector3(-1f * insetFromEdge, 0f, -1f * insetFromEdge), southRotation);
        }
        else if (!carConnectsSouth && !carConnectsWest)
        {
            AddInnerCorner(new Vector3(-1f * insetFromEdge, 0f, -1f * insetFromEdge), westRotation);
        }
    }
}
