using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MrDebug : MonoBehaviour
{
    public List<GameObject> GameCells = new List<GameObject>();

    private GameObject _player;

    void OnInteraction(GameObject interactor)
    {
        _player = interactor;

        var npc = gameObject.GetComponent<NPC>();

        var cellOptions = new List<DialogOption>();

        foreach (var cell in GameCells)
        {
            cellOptions.Add(new DialogOption
            {
                Text = cell.name.Replace("Cell(Clone)", ""),
                Tag = $"start:{cell.name}",
            });
        }

        npc.DialogTree = new DialogNode
        {
            Message = "Howdy, I'm Mr. Debug!",
            Options = new List<DialogOption>
            {
                new DialogOption
                {
                    Text = "Jump to cell",
                    Node = new DialogNode
                    {
                        Message = "<i>select cell</i>",
                        Options = cellOptions,
                    },
                },
            },
        };

        npc.InitiateDialog(interactor);
    }

    void OnDialogChoice(string tag)
    {
        var parts = tag.Split(":");

        if (parts[0] == "start")
        {
            JumpToCell(parts[1]);
        }
    }

    void JumpToCell(string name)
    {
        // TODO: This used to kill "previous" cells (FadeOutAndDie). Not sure what to do post-playground.
        var cell = GameCells.Find(cell => cell.name == name);
        var maybeSpawner = cell.GetComponentInChildren<CarExitSpawner>();
        if (maybeSpawner != null)
        {
            maybeSpawner.SpawnCarExit();
        }

        var gameCell = cell.GetComponent<GameCell>();

        var entryPoint = gameCell.EntryPoint.gameObject.transform.position;

        var mouseController = _player.GetComponent<MouseController>();
        mouseController.Teleport(entryPoint);

        gameCell.PlayerEntered(_player);
    }
}
