using UnityEngine;
using UnityEngine.InputSystem;


namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour, IMouseControllerInput
    {
        public Vector2 move { get; set; }
        public Vector2 look { get; set; }
        public bool jump { get; set; }
        public bool sprint { get; set; }
        public bool interact { get; set; }
        public bool dialogUp { get; set; }
        public bool dialogDown { get; set; }

        public bool analogMovement { get; set; }

        public bool cursorLocked { get; set; } = true;
        public bool cursorInputForLook { get; set; } = true;

        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if(cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        public void OnInteract(InputValue value)
        {
            InteractInput(value.isPressed);
        }

        public void OnDialogUp(InputValue value)
        {
            DialogUpInput(value.isPressed);
        }

        public void OnDialogDown(InputValue value)
        {
            DialogDownInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        } 

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void InteractInput(bool newInteractState)
        {
            interact = newInteractState;
        }

        public void DialogUpInput(bool state)
        {
            dialogUp = state;
        }

        public void DialogDownInput(bool state)
        {
            dialogDown = state;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}
