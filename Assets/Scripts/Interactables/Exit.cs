using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}

[Serializable]
public class CellAttachPoint
{
    public int row;
    public int col;
    public AttachEdge edge;
    public AttachMode mode;
    public GameObject toCell;

    public bool Equals(CellAttachPoint a, CellAttachPoint b)
    {
        return a.row == b.row &&
            a.col == b.col &&
            a.edge == b.edge &&
            a.mode == b.mode;
    }
}

public class Exit : MonoBehaviour
{
    public GameObject FromCell;
    public List<CellAttachPoint> AttachPointOptions;

    public delegate void ExitHandler(GameObject interactor, CellAttachPoint attachPoint);

    private ExitHandler _exitHandler;

    public void SetExitHandler(ExitHandler exitHandler)
    {
        _exitHandler = exitHandler;
    }

    public void AttemptExit(GameObject interactor)
    {
        var fromGameCell = FromCell?.GetComponent<GameCell>();
        if (fromGameCell == null)
        {
            Debug.LogError("Exit missing FromCell");
        }

        if (AttachPointOptions.Count != 1)
        {
            Debug.LogError("TODO: Need to build exit selection dialog.");
            return;
        }

        var attachPoint = AttachPointOptions[0];
        if (!fromGameCell.CanExit(attachPoint))
        {
            return;
        }


        var toCell = attachPoint.toCell?.GetComponent<GameCell>();
        if (toCell == null)
        {
            Debug.LogError("Exit missing to cell");
        }

        if (_exitHandler != null)
        {
            _exitHandler(interactor, attachPoint);
        }

        fromGameCell.PlayerExited(interactor);
    }
}
