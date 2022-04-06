using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Untethered.Characters
{
    [CreateAssetMenu(fileName = "New Standard Ability", menuName = "Untethered/Standard Ability")]
    public class StandardAbility : AbilityBase
    {
        [field: SerializeField, FoldoutGroup("Stat Effects")] public AOEType AOEType {get; private set;}
        [field: SerializeField, FoldoutGroup("Stat Effects")] public ParticleSystem ActivationParticleSystem {get; private set;}

        private RaycastHit _raycastHit;

        internal override void SetUpTriggerForAbility()
        {
            Animation.Events.SetCallback(TRIGGER_DAMAGE, TriggerAbilityAreaOfEffect);
        }

        internal void TriggerAbilityAreaOfEffect()
        {
            List<Character> charactersFound = new List<Character>();
            Vector3 abilityOrigin = _abilityPositioningParent.position;;
            Vector3 abilityDirection = _abilityOwner.transform.forward;

            if (ActivationParticleSystem)
            {
                ActivationParticleSystem.transform.position = abilityOrigin;
                ActivationParticleSystem.Play();
            }
            // if (AOEType == AOEType.Sphere)
            //     _raycastHit = Physics.SphereCastAll(abilityOrigin, )
        }
    }
}
