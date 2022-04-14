using UnityEngine;

namespace Untethered.Characters
{
    [CreateAssetMenu(fileName = "New Particle Collision Ability", menuName = "Untethered/Particle Collision Ability")]
    public class ParticleCollisionAbility : AbilityBase
    {
        internal override void SetUpTriggerForAbilityDamage()
        {
            if (MainParticleSystem)
            {
                OnParticleCollisionCallback particleCollisionCallbacker = MainParticleSystem.GetComponent<OnParticleCollisionCallback>();
                if (particleCollisionCallbacker)
                    particleCollisionCallbacker.OnParticleCollisionEvent.AddListener(HitGameObjectWithAbility);
                else
                    Debug.LogError($"Ability {Name} FAILED to add Hit Event to Particle Collision Callback," +
                                    $" OnParticleCollisionCallback script must be added to particle system prefab {MainParticleSystem.name}");
            }
        }
    }
}
