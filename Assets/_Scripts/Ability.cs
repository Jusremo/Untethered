using System.Collections;
using UnityEngine;
using Animancer;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;
using DG.Tweening;
using Untethered.Utility;

namespace Untethered.Characters
{
    public enum BodyPart { None, LeftHand, RightHand, LeftFoot, RightFoot, WeaponPart, Head, Hips }
    [System.Serializable]
    public struct BodyPartTransform
    {
        [SerializeField] private BodyPart _bodyPart;
        [SerializeField] private Transform _boneTransform;

        public Transform BoneTransform { get => _boneTransform; }
        public BodyPart BodyPart { get => _bodyPart; }
    }

    public enum AOEType {Sphere, Square, Cone}
    public enum Targeting {Self, Allies, Enemies}

    [InlineEditor]
    public abstract class AbilityBase : ScriptableObject
    {        
        protected const string PLAY_MAIN_PARTICLE_SYSTEM = "Play Main Particle System", PLAY_ADDITIONAL_PARTICLE_SYSTEM = "Play Additional Particle System", 
                                TRIGGER_DAMAGE = "Trigger Damage", TRIGGER_MOVEMENT = "Trigger Movement", FINISH_ABILITY = "Finish Ability";


        [field: SerializeField, TabGroup("General")] public string Name  {get; private set; }
        [field: TextArea, SerializeField, TabGroup("General")] public string Description { get; private set; }

        [field:SerializeField, TabGroup("Animation"), EventNames(PLAY_MAIN_PARTICLE_SYSTEM, PLAY_ADDITIONAL_PARTICLE_SYSTEM, TRIGGER_DAMAGE, FINISH_ABILITY, TRIGGER_MOVEMENT)]
        public ClipTransition Animation { get; private set; }

        [field:SerializeField, TabGroup("General")] public BodyPart BodyPartToEmitAbilityFrom {get; private set;}
        
        [SerializeField, TabGroup("Particles")] private ParticleSystem _mainParticleSystemPrefab;
        [SerializeField, TabGroup("Particles")] private List<ParticleSystem> _additionalAnimEventParticleSystemPrefabs;
        [SerializeField, TabGroup("Particles")] private bool _stopParticlesOnFinish, _alignParticlesWithCharacterAim;

        [field: SerializeField, TabGroup("General")] public float Damage { get; private set; }
        [field: SerializeField, TabGroup("General")] public Targeting Targeting {get; private set;}

        public ParticleSystem MainParticleSystem { get; private set; }
        public List<ParticleSystem> AdditionalAnimEventParticleSystems { get; private set; }

        protected Character _abilityOwner;
        protected Transform _abilityPositioningParent;

        protected int _animEventParticleSystemIteration;



        #region Initialization
        
        private void LogMissingAnimEvent(string missingEvent) => Debug.LogWarning($"Ability {Name} is missing required animation event {missingEvent}");

        public virtual void Initialize(Character abilityOwner)
        {
            _abilityOwner = abilityOwner;
            AssignAbilityTransformParent();
            InitializeParticleSystems();
            SetUpTriggerForAbilityDamage();

            if (Animation.Events.GetEventExists(FINISH_ABILITY))
                Animation.Events.SetCallback(FINISH_ABILITY, FinishAbility);
            else LogMissingAnimEvent(FINISH_ABILITY);
            if (Move)
            {
                if (Animation.Events.GetEventExists(TRIGGER_MOVEMENT))
                    Animation.Events.SetCallback(TRIGGER_MOVEMENT, StartMovement);
                else LogMissingAnimEvent(TRIGGER_MOVEMENT);
            } 
        }

        internal virtual void InitializeParticleSystems()
        {
            if (_mainParticleSystemPrefab)
                MainParticleSystem = Instantiate(_mainParticleSystemPrefab);

            if (Animation.Events.GetEventExists(PLAY_MAIN_PARTICLE_SYSTEM))
                Animation.Events.SetCallback(PLAY_MAIN_PARTICLE_SYSTEM, () => StartParticleSystem(MainParticleSystem));
            else LogMissingAnimEvent(PLAY_MAIN_PARTICLE_SYSTEM);

            if (_additionalAnimEventParticleSystemPrefabs.Count == 0) return;

            Animation.Events.SetCallback(PLAY_ADDITIONAL_PARTICLE_SYSTEM, () => StartAnimEventIteratedParticleSystem());

            AdditionalAnimEventParticleSystems = new List<ParticleSystem>();
            foreach (var additionalAnimEventParticleSystem in _additionalAnimEventParticleSystemPrefabs)
                AdditionalAnimEventParticleSystems.Add(Instantiate(additionalAnimEventParticleSystem));
        }

        internal virtual void AssignAbilityTransformParent()
        {
            if (BodyPartToEmitAbilityFrom == BodyPart.None) 
            { 
                _abilityPositioningParent = _abilityOwner.transform; 
                return; 
            }

            _abilityPositioningParent = _abilityOwner.BodyPartsForAbilities.Find(x => x.BodyPart == BodyPartToEmitAbilityFrom).BoneTransform;
            
            if (!_abilityPositioningParent)
                Debug.LogWarning("Ability failed to emit from Body Part " + BodyPartToEmitAbilityFrom + ", " + _abilityOwner.name + " does not have Body Part configured under Body Parts For Abilities!");
        }

        internal virtual void SetUpTriggerForAbilityDamage() {}

        #endregion




        #region Casting Sequence

        internal virtual void BeginCastingAbility() 
        {
            _animEventParticleSystemIteration = 0;
            _abilityOwner.CharacterAnimator.PlayAnimation(Animation);
        }

        internal virtual void StartAnimEventIteratedParticleSystem()
        {
            if (_animEventParticleSystemIteration > AdditionalAnimEventParticleSystems.Count - 1)
            {
                Debug.LogError($"Ability {Name} failed to spawn additional particle system from animation event, make sure animation has same number of additional particle systems and 'Play Additional Particle System' events.");
                return;
            }

            StartParticleSystem(AdditionalAnimEventParticleSystems[_animEventParticleSystemIteration]);
            _animEventParticleSystemIteration++;
        }

        internal virtual void StartParticleSystem(ParticleSystem particleSystem)
        {
            particleSystem.transform.SetParent( _abilityPositioningParent , false);
            particleSystem.transform.forward = _alignParticlesWithCharacterAim ? _abilityOwner.Combat.GetAimPosition() : _abilityOwner.transform.forward;
            particleSystem.Play();
        }

        internal virtual void HitGameObjectWithAbility(GameObject gameObject, Vector3 hitPos) 
        {
            gameObject.TryGetComponent(out Character character);
            if (character) character.Combat.TakeDamage(Damage);
        }

        internal virtual void FinishAbility()
        {
            if (_abilityOwner.Combat.CombatState == CombatState.Attacking)
                _abilityOwner.Combat.SetCombatState(CombatState.None);
            if (_stopParticlesOnFinish)
                MainParticleSystem.Stop();
        }

        #endregion




        #region Movement

        [field: SerializeField, TabGroup("Movement")] public bool Move {get; private set;}
        [field: SerializeField, TabGroup("Movement")] public List<VelocityTween> Movements {get; private set;}

        private void StartMovement()
        {
            Sequence sequence = DOTween.Sequence();
            foreach (var Movement in Movements)
            {
                Vector3 moveDirection = _abilityOwner.transform.TransformDirection(Movement.MoveDirection.Vector3.normalized);
                
                if (Movement.Tween != null && Movement.Tween.active) Movement.Tween.Kill();
                Movement.Tween = DOTween.To(() => _abilityOwner.Rigidbody.velocity, x => _abilityOwner.Rigidbody.velocity = x, moveDirection * Movement.MoveSpeed, Movement.MaxMoveDuration).Pause();
                sequence.Append(Movement.Tween);
            }
            sequence.Play();
        }

        #endregion

    }
}
