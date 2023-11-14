using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCell : MonoBehaviour
{
    public GameObject EntryPoint;

    public void PlayerEntered(GameObject player)
    {
        BroadcastMessage("OnPlayerEnter", player, SendMessageOptions.DontRequireReceiver);
    }
}
