using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    Exit _exit;

    void Start()
    {
        _exit = GetComponent<Exit>();

        _exit.SetExitHandler((interactor, attachPoint) => {
            var toCell = attachPoint.toCell.GetComponent<GameCell>();
            var entryPoint = toCell.EntryPoint.gameObject.transform.position;
            var mouseController = interactor.GetComponent<MouseController>();
            mouseController.Teleport(entryPoint);
            toCell.PlayerEntered(mouseController.gameObject);
        });
    }

    void OnInteraction(GameObject interactor)
    {
        _exit.AttemptExit(interactor);
    }
}
