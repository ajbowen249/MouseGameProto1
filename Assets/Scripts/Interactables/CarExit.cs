using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarExit : MonoBehaviour
{
    public GameObject FromCell;
    public GameObject ToCell;

    public delegate bool ExitRequirement();
    private ExitRequirement _exitRequirement;

    void OnInteraction(GameObject interactor)
    {
        Debug.Log("interacted with car");
        var canExit = _exitRequirement == null ? true : _exitRequirement();
        Debug.Log($"can exit: {canExit}");
    }

    public void SetExitRequirement(ExitRequirement requirement)
    {
        _exitRequirement = requirement;
    }
}
