using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [HideInInspector]
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
    private CellAttachPoint _selectedPoint;
    private GameObject _selectingInteractor;

    private List<CellAttachPoint> SelectableOptions
    {
        get
        {
            return AttachPointOptions.Where(point => point.toCell != null).ToList();
        }
    }

    public void SetExitHandler(ExitHandler exitHandler)
    {
        _exitHandler = exitHandler;
    }

    public void AttemptExit(GameObject interactor)
    {
        AttemptExit(interactor, null);
    }

    public void AttemptExit(GameObject interactor, GameObject previousCell)
    {
        var fromGameCell = FromCell?.GetComponent<GameCell>();
        if (fromGameCell == null)
        {
            Debug.LogError("Exit missing FromCell");
        }

        if (_selectedPoint == null)
        {
            var selectableMinusPrevious = SelectableOptions.Where(point => point.toCell != previousCell).ToList();
            if (selectableMinusPrevious.Count == 1)
            {
                _selectedPoint = selectableMinusPrevious[0];
            }
            else
            {
                StartSelectDialog(interactor);
                return;
            }
        }

        if (!fromGameCell.CanExit(_selectedPoint))
        {
            return;
        }

        var toCell = _selectedPoint.toCell?.GetComponent<GameCell>();
        if (toCell == null)
        {
            Debug.LogError("Exit missing to cell");
        }

        if (_exitHandler != null)
        {
            _exitHandler(interactor, _selectedPoint);
        }

        fromGameCell.PlayerExited(interactor);
        _selectedPoint = null;
    }

    private void StartSelectDialog(GameObject interactor)
    {
        _selectingInteractor = interactor;
        var npc = gameObject.GetComponent<NPC>();

        npc.DialogTree = new DialogNode
        {
            Message = "<i>select direction</i>",
            Options = SelectableOptions.Select((point, index) => new DialogOption
            {
                Text = point.edge.ToString(),
                Tag = index.ToString(),
            }).ToList(),
        };

        if (npc.DialogTree.Options.Count == 0)
        {
            return;
        }

        npc.InitiateDialog(interactor);
    }

    void OnDialogChoice(DialogOption option)
    {
        var index = int.Parse(option.Tag);
        _selectedPoint = SelectableOptions[index];
        AttemptExit(_selectingInteractor);
    }
}
