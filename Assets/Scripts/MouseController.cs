using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using System.Collections;

public interface IMouseControllerInput
{
    public Vector2 move { get; set; }
    public Vector2 look { get; set; }
    public bool jump { get; set; }
    public bool sprint { get; set; }
    public bool interact { get; set; }
    public bool dialogUp { get; set; }
    public bool dialogDown { get; set; }
    public bool analogMovement { get; set; }
    public bool cursorLocked { get; set; }
    public bool cursorInputForLook { get; set; }
}

public class MouseControllerInput : IMouseControllerInput
{
    public Vector2 move { get; set; }
    public Vector2 look { get; set; }
    public bool jump { get; set; }
    public bool sprint { get; set; }
    public bool interact { get; set; }
    public bool dialogUp { get; set; }
    public bool dialogDown { get; set; }
    public bool analogMovement { get; set; }
    public bool cursorLocked { get; set; }
    public bool cursorInputForLook { get; set; }
}

public struct WalkTarget
{
    public Vector3 position;
    public float distance;
    public Action callback;
    public bool? allowIndefinite;
}

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class MouseController : MonoBehaviour
{
    [Header("Character")]
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
    public float GroundedOffset = 0.24f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.92f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    // character
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private MouseAvatar _mouseAvatar;
    private CharacterController _controller;
    private GameObject _mainCamera;

    private InteractionVolume _inInteractionVolume;

    private Vector3? _teleportTo;

    public GameObject AttachedTo { get; private set; }

    public IMouseControllerInput Input { get; set; } = new MouseControllerInput();

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

    public void AttachTo(GameObject parent, Transform target)
    {
        AttachedTo = parent;
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
        AttachedTo = null;
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
        _mouseAvatar = GetComponentInChildren<MouseAvatar>();
        _controller = GetComponent<CharacterController>();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    public void BasicUpdate()
    {
        if (AttachedTo != null || !_controller.enabled)
        {
            return;
        }

        GroundedCheck();
        JumpAndGravity();
        Move();
        Interaction();
    }

    public void WalkTo(WalkTarget target)
    {
        if (_controller == null)
        {
            Debug.LogWarning("Tried to walk an NPC without a controller.");
        }

        _mouseAvatar.CancelEmotes();
        StartCoroutine(WalkToCoroutine(target));
    }

    public void InteractWith(InteractionVolume interactable)
    {
        Action interact = () => interactable.Interact(gameObject);

        if (interactable.EmoteHash is int emoteHash)
        {
            _mouseAvatar.Emote(emoteHash, interact);
        }
        else
        {
            interact();
        }
    }

    private void LateUpdate()
    {
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

        _mouseAvatar.SetGrounded(Grounded);
    }

    private void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = Input.sprint ? SprintSpeed : MoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (Input.move == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = Input.analogMovement ? Input.move.magnitude : 1f;

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
        Vector3 inputDirection = new Vector3(Input.move.x, 0.0f, Input.move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (Input.move != Vector2.zero)
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

        _mouseAvatar.SetWalkRun(_animationBlend, inputMagnitude);
    }

    private void Interaction()
    {
        if (Input.interact)
        {
            Input.interact = false;
            if (_inInteractionVolume != null)
            {
                InteractWith(_inInteractionVolume);
            }
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            _mouseAvatar.SetJumping(false);
            _mouseAvatar.SetFalling(false);

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (Input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                _mouseAvatar.SetJumping(true);
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
                _mouseAvatar.SetFalling(true);
            }

            // if we are not grounded, do not jump
            Input.jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
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

    private IEnumerator WalkToCoroutine(WalkTarget target)
    {
        Vector3 toTarget3D;
        var originalInput = Input;
        Input = new MouseControllerInput();
        var startTime = Time.time;
        var initialDistance = (target.position - transform.position).magnitude;
        var expectedTime = initialDistance / MoveSpeed;
        // Add a slight buffer
        expectedTime *= 1.1f;

        while ((toTarget3D = (target.position - transform.position)).magnitude >= target.distance)
        {
            var toTarget2D = new Vector2(toTarget3D.normalized.x, toTarget3D.normalized.z);
            Input.move = toTarget2D;

            if (target.allowIndefinite != true && Time.time - startTime > expectedTime)
            {
                break;
            }

            yield return null;
        }

        Input = originalInput;
        Input.move = new Vector2(0, 0);
        if (target.callback != null)
        {
            target.callback();
        }
    }
}
