using System;
using System.Collections.Generic;
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

    private List<(AbsoluteAttachPoint point, GridLocation loc)> _attachPoints;

    void OnDeterminedConnections(List<(AbsoluteAttachPoint point, GridLocation loc)> attachPoints)
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

        if (carExits.Count == 4)
        {
            Intersection4Way.SetActive(true);
        }
        else
        {
            var foundMatch = false;

            var roadMapping = new List<(HashSet<AttachEdge> edges, GameObject obj, Vector3 rotation)>
            {
                (new HashSet<AttachEdge> { AttachEdge.EAST, AttachEdge.WEST }, StraightRoad, new Vector3(0, 0, 0)),
                (new HashSet<AttachEdge> { AttachEdge.NORTH, AttachEdge.SOUTH }, StraightRoad, new Vector3(0, 90, 0)),

                (new HashSet<AttachEdge> { AttachEdge.SOUTH, AttachEdge.EAST }, TurnRoad, new Vector3(0, 0, 0)),
                (new HashSet<AttachEdge> { AttachEdge.SOUTH, AttachEdge.WEST }, TurnRoad, new Vector3(0, 90, 0)),
                (new HashSet<AttachEdge> { AttachEdge.WEST, AttachEdge.NORTH }, TurnRoad, new Vector3(0, 180, 0)),
                (new HashSet<AttachEdge> { AttachEdge.EAST, AttachEdge.NORTH }, TurnRoad, new Vector3(0, 270, 0)),

                (new HashSet<AttachEdge> { AttachEdge.WEST, AttachEdge.NORTH, AttachEdge.EAST }, Intersection3Way, new Vector3(0, 0, 0)),
                (new HashSet<AttachEdge> { AttachEdge.NORTH, AttachEdge.EAST, AttachEdge.SOUTH }, Intersection3Way, new Vector3(0, 90, 0)),
                (new HashSet<AttachEdge> { AttachEdge.EAST, AttachEdge.SOUTH, AttachEdge.WEST }, Intersection3Way, new Vector3(0, 180, 0)),
                (new HashSet<AttachEdge> { AttachEdge.SOUTH, AttachEdge.WEST, AttachEdge.NORTH }, Intersection3Way, new Vector3(0, 270, 0)),
            };

            foreach (var mapping in roadMapping)
            {
                if (carExits.SetEquals(mapping.edges))
                {
                    mapping.obj.SetActive(true);
                    var rot = mapping.obj.transform.rotation;
                    mapping.obj.transform.rotation = Quaternion.Euler(rot.eulerAngles + mapping.rotation);
                    foundMatch = true;
                }
            }

            if (!foundMatch)
            {
                UnpaintedRoad.SetActive(true);

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
