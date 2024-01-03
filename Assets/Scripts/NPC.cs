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

    public struct WalkTarget
    {
        public Vector3 position;
        public float distance;
        public Action callback;
    }

    private WalkTarget? _walkingTo;

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

        // Doing this in two separate blocks allows the callback to set a new walk target.
        if (_walkingTo is WalkTarget walkingTo1)
        {
            var toTarget3D = (walkingTo1.position - transform.position);
            var distance = toTarget3D.magnitude;
            if (distance < walkingTo1.distance)
            {
                _walkingTo = null;
                _controller.Input.move = new Vector2(0, 0);
                walkingTo1.callback();
            }
        }

        if (_walkingTo is WalkTarget walkingTo2)
        {
            var toTarget3D = (walkingTo2.position - transform.position);
            var toTarget2D = new Vector2(toTarget3D.normalized.x, toTarget3D.normalized.z);
            _controller.Input.move = toTarget2D;
        }

        _controller.BasicUpdate();
    }

    public void InitiateDialog(GameObject talkingTo)
    {
        if (Avatar != null && !DisableDefaultWaveOnDialog)
        {
            Avatar.Wave();
        }

        talkingTo.BroadcastMessage("OnStartedDialog", gameObject);
    }

    public void WalkTo(WalkTarget target)
    {
        if (_controller == null)
        {
            Debug.LogWarning("Tried to walk an NPC without a controller.");
        }

        _walkingTo = target;
        Avatar.CancelEmotes();
    }
}
