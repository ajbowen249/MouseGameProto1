using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public enum ControlState
{
    EXPLORATION,
    DIALOG,
    SUSPENDED,
}

public struct ActionCost
{
    public string description;
    public float? gas;
    public float? energy;
    public float? time;
    public float? money;
}

public class PlayerMouse : MonoBehaviour
{
    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    public ControlState State { get; private set; } = ControlState.EXPLORATION;

    [HideInInspector]

    public MouseController MouseController;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private const float _threshold = 0.01f;

    private ControlState? _suspendedState;

    private PlayerInput _playerInput;
    private StarterAssetsInputs _input;

    private bool IsCurrentDeviceMouse
    {
        get
        {
            return _playerInput.currentControlScheme == "KeyboardMouse";
        }
    }

    void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        _input = GetComponent<StarterAssetsInputs>();
        _playerInput = GetComponent<PlayerInput>();
        MouseController = GetComponent<MouseController>();
        MouseController.Input = _input;
    }

    void Update()
    {
        if (MouseController.AttachedTo != null)
        {
            return;
        }

        switch (State)
        {
            case ControlState.EXPLORATION:
                MouseController.BasicUpdate();
                break;
            case ControlState.SUSPENDED:
                _input.jump = false;
                _input.interact = false;
                break;
        }
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    public void OnStartedDialog(GameObject talkingTo)
    {
        State = ControlState.DIALOG;
    }

    public void OnEndedDialog(GameObject talkingTo)
    {
        State = ControlState.EXPLORATION;
    }

    public void Suspend(ControlState? returnState = null)
    {
        _suspendedState = returnState ?? State;
        State = ControlState.SUSPENDED;
    }

    public void Resume()
    {
        State = _suspendedState ?? ControlState.EXPLORATION;
        _suspendedState = null;
    }

    public StarterAssetsInputs GetInputController()
    {
        return _input;
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}
