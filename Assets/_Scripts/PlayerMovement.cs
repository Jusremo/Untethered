using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace Untethered.Characters
{
    public enum MoveState {Idle, Walking, Running, Sprinting, Airborne}

    public class PlayerMovement : MonoBehaviour
    {
        [TabGroup("Speeds")]
        [SerializeField] private float _moveSpeed = 6, _sprintSpeed = 9, _moveAcceleration = 10, _stopSpeed = 20, _rotateSpeed = 100, _airSpeed = 3, _airAcceleration = 20;

        [TabGroup("Jump")]
        [SerializeField] private float _jumpForce = 50;

        [TabGroup("Other")]
        [SerializeField] private float _gravityMultiplier = 2;

        [HideInInspector] public UnityEvent<MoveState, MoveState> OnMoveStateChanged = new UnityEvent<MoveState, MoveState>();

        private Player _player;

        public MoveState MoveState { get; private set; }
        public Vector3 MoveDirection { get; private set; }
        public bool Moving { get; private set; }

        private void Awake() 
        {
            _player = GetComponent<Player>();
        }

        private void Start() 
        {
            _player.GroundedChecker.OnGroundedStateChanged.AddListener(ChangeMoveStateOnGroundedChange);
        }

        private void Update() 
        {
            float delta = Time.deltaTime;
            Rotate(delta);
        }

        private void FixedUpdate() 
        {
            float delta = Time.fixedDeltaTime;
            CalculateMovementDirection(delta);

            if (_player.Combat.CombatState == CombatState.None)
            { 
                if (MoveState == MoveState.Airborne) MoveInAir(delta);
                else if (Moving) Move(delta);
                else Stop(delta);
            }

            ApplyGravity(delta);
        }

        #region Basic Movement

        private void ChangeMoveStateOnGroundedChange(GroundedState oldGroundedState, GroundedState newGroundedState)
        {
            SetMoveState(newGroundedState == GroundedState.Grounded ? MoveState.Idle : MoveState.Airborne);
        }

        public void SetMoveState(MoveState moveState)
        {
            if (MoveState == moveState) return;
            MoveState oldMoveState = MoveState;
            MoveState = moveState;
            OnMoveStateChanged.Invoke(oldMoveState, MoveState);
        }

        private void CalculateMovementDirection(float delta)
        {
            Moving = _player.Input.MovementInput.magnitude > 0;
            if (!Moving)
                return;

            Vector3 cameraRight = _player.Camera.right;
            Vector3 cameraFlatForward = Vector3.Cross(-Vector3.up, cameraRight);
            MoveDirection = (_player.Input.MovementInput.x * cameraRight) 
                          + (_player.Input.MovementInput.y * cameraFlatForward);
            MoveDirection.Normalize();
        }
 
        private void MoveInAir(float delta)
        {
            Vector3 velocity = _player.Rigidbody.velocity;
            Vector3 targetVelocity = _airSpeed * MoveDirection;
            targetVelocity.y = velocity.y;
            _player.Rigidbody.velocity = Vector3.MoveTowards(velocity, targetVelocity, _airAcceleration * delta);
        }

        private void Move(float delta)
        {
            Vector3 velocity = _player.Rigidbody.velocity;
            float speed = _player.Input.Sprinting ? _sprintSpeed : _moveSpeed;
            Vector3 targetVelocity = speed * MoveDirection;

            targetVelocity.y = velocity.y;
            _player.Rigidbody.velocity = Vector3.MoveTowards(velocity, targetVelocity, _moveAcceleration * delta);

            SetMoveState(_player.Input.Sprinting ? MoveState.Sprinting : MoveState.Running);
        }   

        private void Stop(float delta)
        {
            Vector3 velocity = _player.Rigidbody.velocity;
            _player.Rigidbody.velocity = Vector3.MoveTowards(velocity, new Vector3(0, velocity.y, 0), _stopSpeed * delta);

            SetMoveState(MoveState.Idle);
        }

        private void ApplyGravity(float delta)
        {
            Vector3 forceToWalkingSurface = Physics.gravity * _gravityMultiplier;
            _player.Rigidbody.velocity += forceToWalkingSurface * delta;
        }     
                 
        private void Rotate(float delta)
        {
            if (!Moving) return;

            Quaternion targetRotation = Quaternion.LookRotation(MoveDirection, Vector3.up);
            Quaternion rotation = Quaternion.RotateTowards(_player.Rigidbody.rotation, targetRotation, _rotateSpeed * delta);
            _player.Rigidbody.MoveRotation(rotation);
        } 

        #endregion
        #region Jump

        public void AttemptToJump()
        {
            if (_player.GroundedChecker.GroundedState != GroundedState.Grounded) return;
            
            _player.CharacterAnimator.PlayAnimation(BasicAnimations.Jumping);
            _player.GroundedChecker.ForceGroundedStateForTime(GroundedState.Falling, 0.5f);
            _player.Rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.VelocityChange);
        }
        
        #endregion
    }
}
