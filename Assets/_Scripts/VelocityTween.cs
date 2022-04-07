using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace Untethered.Utility
{
    [System.Serializable]
    public class VelocityTween 
    {
        [field: SerializeField] public RangeVector3 MoveDirection {get; private set;}
        [field: SerializeField] public Ease MovementEase {get; private set;} = Ease.Linear;
        [field: SerializeField] public bool MovementTracksTarget {get; private set;}
        [field: SerializeField] public float MoveSpeed {get; private set;}
        [field: SerializeField] public float MaxMoveDuration {get; private set;}

        [HideInInspector] public Tween Tween;
    }
}
