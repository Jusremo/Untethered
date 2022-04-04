using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Events;

namespace Untethered.Characters
{
    public enum GroundedState {Grounded, Falling, Landing}
    public class GroundedChecker : MonoBehaviour
    {
        [TabGroup("Raycasting"), SerializeField] private float _raycastGroundOffset = 0.5f;
        [TabGroup("Raycasting"), SerializeField] private float _rayDistance = 0.65f;
        [TabGroup("Raycasting"), SerializeField] private float _feetWidth = 0.25f;
        [TabGroup("Raycasting"), SerializeField] private float _groundCheckInterval = 0.2f;

        [TabGroup("Falling"), SerializeField] private float _rayDistanceMultiplierWhileFalling = 3;
        [TabGroup("Falling"), SerializeField, Tooltip("Ray distance multiplier will max out at this downwards velocity")] private float _rayDistanceMultiplierMaxFallingVelocity = 5;

        [HideInInspector] public UnityEvent<GroundedState, GroundedState> OnGroundedStateChanged = new UnityEvent<GroundedState, GroundedState>();

        public Vector3 GroundNormal { get; private set; }
        public GroundedState GroundedState  { get; private set; }

        private RaycastHit raycastHit; 

        private float _groundCheckDelayTimer;
        private Player _player;

        private void Awake() 
        {
            _player = GetComponent<Player>();
        }
        
        private IEnumerator Start() 
        {
            while (true)
            {
                yield return new WaitForSeconds(_groundCheckInterval);
                RaycastForGround();
            }
        }

        private void RaycastForGround()
        {
            if (_groundCheckDelayTimer > Time.time) return;

            float rayDist = _rayDistance;
            if (GroundedState == GroundedState.Falling || GroundedState == GroundedState.Landing)
                rayDist *= GetFallSpeedRayDistanceMultiplier(_player.Rigidbody.velocity.y);
            
            bool hitGround = Physics.BoxCast(transform.position + (Vector3.up * _raycastGroundOffset), Vector3.one * _feetWidth, -Vector3.up, out raycastHit, Quaternion.identity, rayDist, ~(int)LayerMasks.Characters);

            if (hitGround)
            {
                if (GroundedState == GroundedState.Falling)
                {
                    SetGrounded(GroundedState.Landing);
                }
                else if (GroundedState == GroundedState.Landing)
                {
                    if ((raycastHit.distance - _raycastGroundOffset) < _rayDistance) 
                        SetGrounded(GroundedState.Grounded);
                }

            }
            else SetGrounded(GroundedState.Falling);
        }

        private float GetFallSpeedRayDistanceMultiplier(float fallSpeed)
        {
            fallSpeed = Mathf.Max(fallSpeed, -_rayDistanceMultiplierMaxFallingVelocity);
            if (fallSpeed < 0)
            {
                float fallSpeedRayMultiplier = Mathf.Min((fallSpeed / _rayDistanceMultiplierMaxFallingVelocity) * _rayDistanceMultiplierWhileFalling, -1);
                return Mathf.Abs(fallSpeedRayMultiplier);
            }
            return 1;
        }

        private void SetGrounded(GroundedState newState)
        {
            if (GroundedState == newState) return;
            GroundedState oldState = GroundedState;
            GroundedState = newState;
            OnGroundedStateChanged.Invoke(oldState, GroundedState);
        }

        public void ForceGroundedStateForTime(GroundedState state, float delay)
        {
            SetGrounded(state);
            _groundCheckDelayTimer = delay + Time.time;
        }

        private void OnDrawGizmos() => DrawRaycast();

        private void DrawRaycast()
        {
            Gizmos.color = Color.green;
            Vector3 startPos = transform.position + (Vector3.up * _raycastGroundOffset);
            Vector3 endPos = startPos + (-Vector3.up * _rayDistance);

            Gizmos.DrawSphere(startPos, 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endPos, 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPos, endPos);

            float rayDist = _rayDistance;
            if (GroundedState == GroundedState.Falling || GroundedState == GroundedState.Landing)
                rayDist *= GetFallSpeedRayDistanceMultiplier(_player.Rigidbody.velocity.y);

            BoxUtility.DrawBoxCastBox(transform.position + (Vector3.up * _raycastGroundOffset), Vector3.one * _feetWidth, -Vector3.up, Quaternion.identity, rayDist, Color.yellow);
        }
    }
}
