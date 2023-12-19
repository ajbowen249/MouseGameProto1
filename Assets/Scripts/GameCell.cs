using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public enum AttachEdge
{
    NORTH,
    SOUTH,
    EAST,
    WEST,
}

public static class AttachEdgeExtensions
{
    public static AttachEdge Opposite(this AttachEdge edge)
    {
        switch (edge)
        {
        case AttachEdge.NORTH:
            return AttachEdge.SOUTH;
        case AttachEdge.SOUTH:
            return AttachEdge.NORTH;
        case AttachEdge.EAST:
            return AttachEdge.WEST;
        case AttachEdge.WEST:
            return AttachEdge.EAST;
        default:
            throw new Exception("Unknown edge value");
        }
    }
}

[Serializable]
public class CellFootprint
{
    public int row;
    public int col;
}

[Serializable]
public enum Biome
{
    URBAN,
    SUBURBAN,
    ROAD,
}

public class GameCell : MonoBehaviour
{
    public const float CellWidth = 20;
    public const float HalfWidth = CellWidth / 2;

    public GameObject EntryPoint;
    public List<Biome> Biomes;
    public List<CellFootprint> Footprint;
    public bool CanBeRandom;
    public List<CellAttachPoint> EntryPoints;
    public bool DestroyOnExit = false;

    [HideInInspector]
    public int Row { get; set; }
    public int Col { get; set; }

    public delegate bool ExitRequirement(CellAttachPoint attachPoint);
    private ExitRequirement _exitRequirement;

    public IEnumerable<CellAttachPoint> AllExitPoints
    {
        get
        {
            return GetComponentsInChildren<Exit>(true)
                .SelectMany(exit => exit.AttachPointOptions);
        }
    }

    public IEnumerable<CellAttachPoint> ActiveExitPoints
    {
        get
        {
            return GetComponentsInChildren<Exit>()
                .SelectMany(exit => exit.AttachPointOptions);
        }
    }

    public IEnumerable<CellAttachPoint> AllAttachPoints
    {
        get
        {
            var points = AllExitPoints.ToList();
            points.AddRange(EntryPoints);

            return points;
        }
    }

    public IEnumerable<CellAttachPoint> ActiveAttachPoints
    {
        get
        {
            var points = ActiveExitPoints.ToList();
            points.AddRange(EntryPoints);

            return points;
        }
    }

    public IEnumerable<CellAttachPoint> DistinctAttachPoints
    {
        get
        {
            return AllAttachPoints.Distinct();
        }
    }

    private IEnumerable<CellAttachPoint> ActiveCarExits
    {
        get
        {
            return ActiveExitPoints.Where(point =>point.toCell != null && point.mode.type == AttachModeType.CAR);
        }
    }

        private IEnumerable<CellAttachPoint> ActiveFootExits
    {
        get
        {
            return ActiveExitPoints.Where(point => point.toCell != null && point.mode.type == AttachModeType.FOOT);
        }
    }

    public void SetExitRequirement(ExitRequirement requirement)
    {
        _exitRequirement = requirement;
    }

    public bool CanExit(CellAttachPoint attachPoint)
    {
        return _exitRequirement == null ? true : _exitRequirement(attachPoint);
    }

    public void PlayerEntered(GameObject player)
    {
        BroadcastMessage("OnPlayerEnter", player, SendMessageOptions.DontRequireReceiver);
    }

    public void PlayerExited(GameObject player)
    {
        BroadcastMessage("OnPlayerExit", player, SendMessageOptions.DontRequireReceiver);
        if (DestroyOnExit)
        {
            FadeOutAndDie();
        }
    }

    public void DeterminedConnections(List<(AbsoluteAttachPoint point, int row, int col)> attachPoints)
    {
        foreach (var exit in GetComponentsInChildren<Exit>())
        {
            exit.AttachPointOptions = exit.AttachPointOptions.Where(option => !option.mode.isOptional || attachPoints.Any(
                point => point.row == option.row && point.col == option.col &&
                    point.point.edge == option.edge && point.point.modeType == option.mode.type
            )).ToList();
        }

        BroadcastMessage("OnDeterminedConnections", attachPoints, SendMessageOptions.DontRequireReceiver);
    }

    public void GenerationComplete()
    {
        BroadcastMessage("OnGenerationComplete", this, SendMessageOptions.DontRequireReceiver);
    }

    public bool CanAutoContinue(GameObject fromCell)
    {
        return ActiveCarExits.Where(point => point.toCell != fromCell).Count() == 1 &&
            ActiveFootExits.Count() == 0 && _exitRequirement == null;
    }

    public void FadeOutAndDie()
    {
        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        foreach (var collider in gameObject.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        var renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
        float steps = 10.0f;

        for (float i = 0.0f; i < steps; i++)
        {
            var progress = (steps - (i / 2)) / steps;
            var color = new Color(progress, progress, progress, progress);

            foreach (var renderer in renderers)
            {
                renderer.material.color = color;
            }

            yield return new WaitForSeconds(0.05f);
        }

        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        foreach (var attachPoint in DistinctAttachPoints)
        {
            var x = CellWidth * (float)attachPoint.col;
            var z = CellWidth * (float)attachPoint.row;

            switch (attachPoint.edge)
            {
            case AttachEdge.NORTH:
                z += HalfWidth;
                break;
            case AttachEdge.SOUTH:
                z -= HalfWidth;
                break;
            case AttachEdge.EAST:
                x += HalfWidth;
                break;
            case AttachEdge.WEST:
                x -= HalfWidth;
                break;
            }

            Gizmos.DrawWireSphere(transform.position + new Vector3(x, HalfWidth / 2, z), 1);
        }

        Gizmos.color = Color.green;

        foreach (var footprint in Footprint)
        {
            Gizmos.DrawWireCube(
                transform.position + new Vector3(
                    CellWidth * footprint.col,
                    HalfWidth / 2,
                    CellWidth * footprint.row
                ),
                new Vector3(CellWidth, HalfWidth, CellWidth)
            );
        }
    }
}
