using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCell : MonoBehaviour
{
    public GameObject EntryPoint;

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
}
