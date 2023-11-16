using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public GameObject FromCell;
    public GameObject ToCell;

    void OnInteraction(GameObject interactor)
    {
        if (ToCell == null)
        {
            Debug.LogWarning("Interacted with empty door");
            return;
        }

        var gameCell = ToCell.GetComponent<GameCell>();
        var entryPoint = gameCell.EntryPoint.gameObject.transform.position;

        var mouseController = interactor.GetComponent<MouseController>();
        mouseController.Teleport(entryPoint);
        gameCell.PlayerEntered(mouseController.gameObject);

        if (FromCell != null)
        {
            FromCell.GetComponent<GameCell>().FadeOutAndDie();
        }
    }
}
