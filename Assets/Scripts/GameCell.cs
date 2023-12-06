using System;
using System.Collections;
using System.Collections.Generic;
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
public enum AttachModeType
{
    CAR,
    FOOT,
}

[Serializable]
public class AttachMode
{
    public AttachModeType type;
    public bool isOptional;
    public List<GameObject> exitObjects;
}

[Serializable]
public class CellAttachPoint
{
    public int row;
    public int col;
    public AttachEdge edge;
    public List<AttachMode> modes;
}

[Serializable]
public class CellFootprint
{
    public int row;
    public int col;
}

public class GameCell : MonoBehaviour
{
    public const float CellWidth = 20;
    public const float HalfWidth = CellWidth / 2;

    public GameObject EntryPoint;
    public List<CellAttachPoint> AttachPoints;
    public List<CellFootprint> Footprint;
    public bool CanBeRandom;

    public delegate bool ExitRequirement();
    private ExitRequirement _exitRequirement;

    public void SetExitRequirement(ExitRequirement requirement)
    {
        _exitRequirement = requirement;
    }

    public bool CanExit()
    {
        return _exitRequirement == null ? true : _exitRequirement();
    }

    public void PlayerEntered(GameObject player)
    {
        BroadcastMessage("OnPlayerEnter", player, SendMessageOptions.DontRequireReceiver);
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

        foreach (var attachPoint in AttachPoints)
        {
            var x = CellWidth * (float)attachPoint.col;
            var z = CellWidth * (float)attachPoint.row;

            switch (attachPoint.edge)
            {
            case AttachEdge.SOUTH:
                z -= HalfWidth;
                break;
            case AttachEdge.NORTH:
                z += HalfWidth;
                break;
            case AttachEdge.EAST:
                x -= HalfWidth;
                break;
            case AttachEdge.WEST:
                x += HalfWidth;
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
