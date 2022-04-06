using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Untethered.Characters
{
    public class ControlsInput : MonoBehaviour
    {
        private Player _player;

        private InputActions _inputActions;
        public Vector2 MovementInput {get; private set;}

        private void Awake() 
        {
            Cursor.lockState = CursorLockMode.Locked;
            _player = GetComponent<Player>();
            _inputActions = new InputActions();
            
            _inputActions.Player.Jump.performed += JumpInput;
            _inputActions.Player.AbilityOne.performed += AbilityOneInput;

            _inputActions.Player.Enable();
        }

        private void AbilityOneInput(InputAction.CallbackContext obj)
        {
            _player.Combat.AttemptToFireAbility(0);
        }

        private void JumpInput(InputAction.CallbackContext obj)
        {
            _player.Movement.AttemptToJump();
        }

        private void Update() 
        {
            MovementInputRead();
        }

        private void MovementInputRead()
        {
            MovementInput = _inputActions.Player.Movement.ReadValue<Vector2>();
        }
    }
}
