using System.Collections;
using UnityEngine;
using Animancer;
using System.Collections.Generic;
using System;
using Sirenix.OdinInspector;
using DG.Tweening;

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

        [field: SerializeField, TabGroup("General")] public float Damage { get; private set; }
        [field: SerializeField, TabGroup("General")] public Targeting Targeting {get; private set;}

        [field: SerializeField, TabGroup("Movement")] public bool Move {get; private set;}
        [field: SerializeField, TabGroup("Movement"), ShowIf("Move")] public float MoveDistance {get; private set;}
        [field: SerializeField, TabGroup("Movement"), ShowIf("Move")] public float MoveDuration {get; private set;}
        [field: SerializeField, TabGroup("Movement"), ShowIf("Move")] public Ease MovementEase {get; private set;} = Ease.Linear;
        [field: SerializeField, TabGroup("Movement"), ShowIf("Move")] public bool MovementTracksTarget {get; private set;}

        public ParticleSystem MainParticleSystem { get; private set; }
        public List<ParticleSystem> AdditionalAnimEventParticleSystems { get; private set; }

        protected Character _abilityOwner;
        protected Transform _abilityPositioningParent;

        protected int _animEventParticleSystemIteration;

        public virtual void Initialize(Character abilityOwner)
        {
            _abilityOwner = abilityOwner;
            AssignAbilityTransformParent();
            InitializeParticleSystems();
            SetUpTriggerForAbility();

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
        
        private void LogMissingAnimEvent(string missingEvent) => Debug.LogWarning($"Ability {Name} is missing required animation event {missingEvent}");

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

        internal virtual void SetUpTriggerForAbility() {}

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
            particleSystem.transform.forward = _abilityOwner.Combat.GetAimPosition();
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
        }

        private void StartMovement()
        {
            Vector3 targetPos = _abilityOwner.transform.position + (_abilityOwner.transform.forward * MoveDistance);
            _abilityOwner.Rigidbody.DOMove(targetPos, MoveDuration).SetEase(MovementEase);
        }

    }
}
