using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem.XR;

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
    private int _animIDWaving;

    private void Start()
    {
        _animator = GetComponent<Animator>();

        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        _animIDWaving = Animator.StringToHash("Waving");

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

    public void Wave()
    {
        _animator.SetBool(_animIDWaving, true);
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
        _animator.SetBool(_animIDWaving, false);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
    }

    private void OnLand(AnimationEvent animationEvent)
    {
    }

    private void OnWaveEnd()
    {
        _animator.SetBool(_animIDWaving, false);
    }
}
