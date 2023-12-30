using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using System.Collections.Generic;

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

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class MouseController : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

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

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    private ControlState? _suspendedState;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    private PlayerInput _playerInput;
    private Animator _animator;
    private CharacterController _controller;
    private StarterAssetsInputs _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private InteractionVolume _inInteractionVolume;

    private Vector3? _teleportTo;

    private GameObject _attachedTo;

    private bool IsCurrentDeviceMouse
    {
        get
        {
            return _playerInput.currentControlScheme == "KeyboardMouse";
        }
    }

    public float Gas { get; private set; } = 20f;
    public float Energy { get; private set; } = 100f;
    public float Hours { get; private set; } = 5f;
    public float Money { get; private set; } = 100f;

    public Dictionary<string, int> Inventory { get; private set; } = new Dictionary<string, int>();

    public void Teleport(Vector3 position)
    {
        _teleportTo = position;
    }

    public void ExpendResources(ActionCost cost)
    {
        var message = $"{cost.description}";
        var costText = "";

        if (cost.gas is float gas)
        {
            Gas -= gas;
            costText += $" G: {gas} ";
        }

        if (cost.energy is float energy)
        {
            Energy -= energy;
            costText += $" E: {energy} ";
        }

        if (cost.time is float time)
        {
            Hours -= time;
            costText += $" T: {FormatUtils.FormatHours(time)} ";
        }

        if (cost.money is float money)
        {
            Money -= money;
            costText += $" {FormatUtils.FormatMoney(money)} ";
        }

        if (costText != "")
        {
            message += $":{costText}";
            HUD.Instance.SetMeters(Gas, Energy, Hours, Money);
        }

        HUD.Instance.AddMessage(message);
    }

    public void AddInventoryItem(string name, int quantity)
    {
        int existingQuantity = 0;
        Inventory.TryGetValue(name, out existingQuantity);
        existingQuantity += quantity;
        Inventory[name] = existingQuantity;

        HUD.Instance.AddMessage($"Picked up {name} (x{quantity})");
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

    public void AttachTo(GameObject parent, Transform target)
    {
        _attachedTo = parent;
        _controller.enabled = false;
        transform.SetParent(parent.transform);
        transform.position = target.position;
        transform.rotation = target.rotation;
    }

    public void DetachFrom(Transform target)
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
        transform.SetParent(null);
        _controller.enabled = true;
        _attachedTo = null;
    }

    public bool CanSelectDialogOption(DialogOption option)
    {
        if (option.Cost?.gas is float gas && Gas < gas)
        {
            return false;
        }

        if (option.Cost?.energy is float energy && Energy < energy)
        {
            return false;
        }

        if (option.Cost?.time is float time && Hours < time)
        {
            return false;
        }

        return true;
    }

    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        _animator = GetComponentInChildren<Animator>();

        AssignAnimationIDs();


        _controller = GetComponent<CharacterController>();
        _input = GetComponent<StarterAssetsInputs>();
        _playerInput = GetComponent<PlayerInput>();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void Update()
    {
        if (_attachedTo != null)
        {
            return;
        }

        switch (State)
        {
            case ControlState.EXPLORATION:
                GroundedCheck();
                JumpAndGravity();
                Move();
                Interaction();
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

        if (_teleportTo != null)
        {
            _controller.enabled = false;
            _controller.transform.position = (Vector3)_teleportTo;
            _controller.enabled = true;
            _teleportTo = null;
        }
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(
            transform.position.x,
            transform.position.y - GroundedOffset,
            transform.position.z
        );

        Grounded = Physics.CheckSphere(
            spherePosition,
            GroundedRadius,
            GroundLayers,
            QueryTriggerInteraction.Ignore
        );

        _animator.SetBool(_animIDGrounded, Grounded);
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

    private void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f)
        {
            _animationBlend = 0f;
        }

        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (_input.move != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }


        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

        // move the player
        _controller.Move(
            targetDirection.normalized * (_speed * Time.deltaTime) +
                new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime
        );

        _animator.SetFloat(_animIDSpeed, _animationBlend);
        _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
    }

    private void Interaction()
    {
        if (_input.interact)
        {
            _input.interact = false;
            if (_inInteractionVolume != null)
            {
                _inInteractionVolume.Interact(gameObject);
                _inInteractionVolume = null;
            }
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            _animator.SetBool(_animIDJump, false);
            _animator.SetBool(_animIDFreeFall, false);

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                _animator.SetBool(_animIDJump, true);
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _animator.SetBool(_animIDFreeFall, true);
            }

            // if we are not grounded, do not jump
            _input.jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactionVolume = other.gameObject.GetComponent<InteractionVolume>();
        if (interactionVolume != null)
        {
            _inInteractionVolume = interactionVolume;
            HUD.Instance.SetInteractionPrompt(_inInteractionVolume.Prompt);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var interactionVolume = other.gameObject.GetComponent<InteractionVolume>();
        if (interactionVolume != null)
        {
            _inInteractionVolume = null;
            HUD.Instance.ClearInteractionPrompt();
        }
    }
}
