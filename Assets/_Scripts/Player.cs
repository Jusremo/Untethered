using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Untethered.Characters
{
    public class Player : MonoBehaviour
    {
        [field: SerializeField] public Transform Camera { get; private set; }
        [field: SerializeField] public CharacterAnimator CharacterAnimator { get; private set; }

        public PlayerMovement Movement { get; private set; }
        public GroundedChecker GroundedChecker { get; private set; }
        public ControlsInput Input { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        public Collider Collider { get; private set; }
        public CharacterController CharacterController { get; private set; }

        private void Awake() 
        {
            Movement = GetComponent<PlayerMovement>();
            GroundedChecker = GetComponent<GroundedChecker>();
            Input = GetComponent<ControlsInput>();
            Rigidbody = GetComponent<Rigidbody>();
            Collider = GetComponent<Collider>();
            CharacterController = GetComponent<CharacterController>();
        }
    }
}
