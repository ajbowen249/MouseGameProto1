using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class NPC : MonoBehaviour
{
    public GameObject TalkCamera;

    public DialogNode DialogTree { get; set; }

    void Start()
    {
        TalkCamera.SetActive(false);
    }

    public void InitiateDialog(GameObject talkingTo)
    {
        talkingTo.BroadcastMessage("OnStartedDialog", gameObject);
    }
}
