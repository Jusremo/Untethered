using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace Untethered.Characters
{
    public class Character : MonoBehaviour
    {
        [field:SerializeField] public List<BodyPartTransform> BodyPartsForAbilities { get; protected set; }
        [field:SerializeField] public CharacterAnimator CharacterAnimator { get; protected set; }

        public Rigidbody Rigidbody { get; protected set; }
        public Collider Collider { get; protected set; }
        public GroundedChecker GroundedChecker { get; protected set; }
        public Combat Combat { get; protected set; }

        internal virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            Collider = GetComponent<Collider>();
            GroundedChecker = GetComponent<GroundedChecker>();
            Combat = GetComponent<Combat>();
        }
    }
}
