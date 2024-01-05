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

    public void WalkTo(WalkTarget target)
    {
        if (_controller == null)
        {
            Debug.LogWarning("Tried to walk an NPC without a controller.");
        }

        Avatar.CancelEmotes();
        StartCoroutine(WalkToCoroutine(target));
    }

    private IEnumerator WalkToCoroutine(WalkTarget target)
    {
        Vector3 toTarget3D;
        while ((toTarget3D = (target.position - transform.position)).magnitude >= target.distance)
        {
            var toTarget2D = new Vector2(toTarget3D.normalized.x, toTarget3D.normalized.z);
            _controller.Input.move = toTarget2D;
            yield return null;
        }

        _controller.Input.move = new Vector2(0, 0);
        if (target.callback != null)
        {
            target.callback();
        }
    }
}
