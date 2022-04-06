using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Untethered.Characters
{
    public class Player : Character
    {
        [field: SerializeField] public Transform Camera { get; private set; }

        public PlayerMovement Movement { get; private set; }
        public ControlsInput Input { get; private set; }

        internal override void Awake() 
        {
            base.Awake();
            Movement = GetComponent<PlayerMovement>();
            Input = GetComponent<ControlsInput>();
        }
    }
}
