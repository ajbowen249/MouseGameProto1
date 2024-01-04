using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public static class MouseEmotes
{
    public static int Wave = Animator.StringToHash("Wave");
    public static int NondescriptAction1 = Animator.StringToHash("NondescriptAction1");

    public static List<int> AllEmotes = new List<int> { Wave, NondescriptAction1 };
}

public class MouseAvatar : MonoBehaviour
{
    [SerializeField]
    public Color Tint = new Color(0, 0, 0);

    private Animator _animator;

    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private Action _emoteEndCallback;

    private void Start()
    {
        _animator = GetComponent<Animator>();

        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");

        ApplyTint();
    }

    public void SetGrounded(bool grounded)
    {
        _animator.SetBool(_animIDGrounded, grounded);
    }

    public void SetJumping(bool jumping)
    {
        _animator.SetBool(_animIDJump, jumping);
    }

    public void SetFalling(bool falling)
    {
        _animator.SetBool(_animIDFreeFall, falling);
    }

    public void SetWalkRun(float blend, float magnitude)
    {
        _animator.SetFloat(_animIDSpeed, blend);
        _animator.SetFloat(_animIDMotionSpeed, magnitude);
    }
    public void Emote(int emoteHash)
    {
        Emote(emoteHash, () => { });
    }

    public void Emote(int emoteHash, Action callback)
    {
        _animator.SetBool(emoteHash, true);
        _emoteEndCallback = callback;
    }

    public void ApplyTint()
    {
        var renderer = GetComponentInChildren<Renderer>();
        var block = new MaterialPropertyBlock();
        block.SetColor("_Tint", Tint);
        renderer.SetPropertyBlock(block);
    }

    public void CancelEmotes()
    {
        foreach (var emoteHash in MouseEmotes.AllEmotes)
        {
            _animator.SetBool(emoteHash, false);
        }
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
    }

    private void OnLand(AnimationEvent animationEvent)
    {
    }

    private void OnEmoteEnd(string name)
    {
        _animator.SetBool(Animator.StringToHash(name), false);
        NotifyEmoteComplete();
    }

    private void NotifyEmoteComplete()
    {
        if (_emoteEndCallback != null)
        {
            var callback = _emoteEndCallback;
            _emoteEndCallback = null;
            callback();
        }
    }
}
