using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private InteractionVolume _inInteractionVolume;

    [HideInInspector]
    public StarterAssetsInputs Input { get; private set; }

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
        Input = GetComponent<StarterAssetsInputs>();
        _playerInput = GetComponent<PlayerInput>();
        MouseController = GetComponent<MouseController>();
        MouseController.Input = Input;
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
                Interaction();
                break;
            case ControlState.SUSPENDED:
                Input.jump = false;
                Input.interact = false;
                break;
        }
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void Interaction()
    {
        if (Input.interact)
        {
            Input.interact = false;
            if (_inInteractionVolume != null)
            {
                MouseController.InteractWith(_inInteractionVolume);
            }
        }
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
        return Input;
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (Input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += Input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += Input.look.y * deltaTimeMultiplier;
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

    private void OnTriggerEnter(Collider other)
    {
        var interactionVolume = other.gameObject.GetComponent<InteractionVolume>();
        if (interactionVolume != null)
        {
            _inInteractionVolume = interactionVolume;
            HUD.WithInstance(hud => hud.SetInteractionPrompt(_inInteractionVolume.Prompt));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var interactionVolume = other.gameObject.GetComponent<InteractionVolume>();
        if (interactionVolume != null)
        {
            _inInteractionVolume = null;
            HUD.WithInstance(hud => hud.ClearInteractionPrompt());
        }
    }
}
