using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using System;
using Sirenix.OdinInspector;

namespace Untethered.Characters
{
    public enum CharacterAnimations {Jumping, Landing}
    public class CharacterAnimator : SerializedMonoBehaviour
    {
        [SerializeField] private Player _player;
        [SerializeField] private float _transitionSpeed = 0.1f;
        [SerializeField] private Dictionary<MoveState, AnimationClip> _movementAnims;
        [SerializeField] private Dictionary<CharacterAnimations, ClipTransition> _characterAnims;
        [FoldoutGroup("Attacks")]
        [EventNames("Hit", "Finish")]
        [SerializeField] private ClipTransition _clipTest;

        private AnimancerComponent _animancer;
        private AnimancerState _currentAnimState;
        
        private void Awake() 
        {
            _animancer = GetComponent<AnimancerComponent>();
        }

        private void Start() 
        {
            _animancer.Play(_movementAnims[MoveState.Idle], _transitionSpeed);
            _player.Movement.OnMoveStateChanged.AddListener(OnMoveStateChanged);
            _player.GroundedChecker.OnGroundedStateChanged.AddListener(OnGroundedStateChanged);
        }

        public void PlayAnimation(AnimationClip anim)
        {
            if (_currentAnimState != null) _currentAnimState.Events.OnEnd -= RevertToDefaultMovementAnim;

            _currentAnimState = _animancer.Play(anim, _transitionSpeed);
            _currentAnimState.Events.OnEnd += RevertToDefaultMovementAnim;
        }

        public void PlayAnimation(CharacterAnimations anim)
        {
            if (!_characterAnims.TryGetValue(anim, out ClipTransition animClip)) return;
            
            print(anim);
            _currentAnimState = _animancer.Play(animClip, _transitionSpeed);
            _currentAnimState.Events.OnEnd += RevertToDefaultMovementAnim;
        }

        private void RevertToDefaultMovementAnim()
        {
            if (_currentAnimState != null) _currentAnimState.Events.OnEnd -= RevertToDefaultMovementAnim;
            _animancer.Play(_movementAnims[_player.Movement.MoveState], _transitionSpeed);
            _currentAnimState = null;
        }

        private void OnMoveStateChanged(MoveState oldMoveState, MoveState newMoveState) 
        {
            if (_currentAnimState == null)
                _animancer.Play(_movementAnims[newMoveState], _transitionSpeed);
        }

        private void OnGroundedStateChanged(GroundedState oldGroundedState, GroundedState newGroundedState)
        {
            if (newGroundedState == GroundedState.Landing)
                PlayAnimation(CharacterAnimations.Landing);
        }
    }
}