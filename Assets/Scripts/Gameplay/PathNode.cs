using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    private PathNode _previous;
    private PathNode _next;

    void ScanSiblings()
    {
        var siblings = transform.parent.GetComponentsInChildren<PathNode>();
        var index = Array.IndexOf(siblings, this);

        if (index < 0)
        {
            return;
        }

        if (index > 0)
        {
            _previous = siblings[index - 1];
        }

        if (index < siblings.Length - 1)
        {
            _next = siblings[index + 1];
        }
    }

    void OnDrawGizmos()
    {
        ScanSiblings();
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 1);

        if (_next != null)
        {
            Gizmos.DrawLine(transform.position, _next.transform.position);
        }
    }
}
