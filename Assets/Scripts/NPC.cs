using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using System;

public class NPC : MonoBehaviour
{
    public GameObject TalkCamera;

    public MouseAvatar Avatar { get; private set; }

    public DialogNode DialogTree { get; set; }

    public bool DisableDefaultWaveOnDialog;

    private MouseController _controller;

    void Start()
    {
        _controller = GetComponent<MouseController>();

        if (TalkCamera != null)
        {
            TalkCamera.SetActive(false);
        }

        Avatar = GetComponentInChildren<MouseAvatar>();
    }


    public void Update()
    {
        if (_controller == null)
        {
            return;
        }

        _controller.BasicUpdate();
    }

    public void InitiateDialog(GameObject talkingTo)
    {
        if (Avatar != null && !DisableDefaultWaveOnDialog)
        {
            Avatar.Emote(MouseEmotes.Wave);
        }

        talkingTo.BroadcastMessage("OnStartedDialog", gameObject);
    }
}
