using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class NPC : MonoBehaviour
{
    public GameObject TalkCamera;

    public MouseAvatar Avatar { get; private set; }

    public DialogNode DialogTree { get; set; }

    public bool DisableDefaultWaveOnDialog;

    void Start()
    {
        if (TalkCamera != null)
        {
            TalkCamera.SetActive(false);
        }

        Avatar = GetComponentInChildren<MouseAvatar>();
    }


    public void Update()
    {
        if (Avatar != null)
        {
            Avatar.SetGrounded(false);
            Avatar.SetFalling(false);
            Avatar.SetJumping(false);
            Avatar.SetWalkRun(0, 1);
        }
    }

    public void InitiateDialog(GameObject talkingTo)
    {
        if (Avatar != null && !DisableDefaultWaveOnDialog)
        {
            Avatar.Wave();
        }

        talkingTo.BroadcastMessage("OnStartedDialog", gameObject);
    }
}
