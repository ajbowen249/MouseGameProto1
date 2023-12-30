using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class MouseAvatar : MonoBehaviour
{
    private Animator _animator;

    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private void Start()
    {
        _animator = GetComponent<Animator>();

        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
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

    private void OnFootstep(AnimationEvent animationEvent)
    {
    }

    private void OnLand(AnimationEvent animationEvent)
    {
    }
}
