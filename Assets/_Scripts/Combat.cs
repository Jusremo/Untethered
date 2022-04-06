using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Events;

namespace Untethered.Characters
{
    public enum CombatState {None, Attacking, Stunned}
    public class Combat : MonoBehaviour
    {
        [TabGroup("Basic Stats")]
        [field: SerializeField] private float _maxHealth = 100, _maxStamina = 100;
        [field:SerializeField] public List<AbilityBase> Abilities { get; private set; }

        public UnityEvent<CombatState, CombatState> OnCombatStateChanged = new UnityEvent<CombatState, CombatState>();

        protected Character _character;

        public CombatState CombatState { get; private set; }
        public float CurrentHealth {get; private set; }
        public float CurrentStamina {get; private set; }

        private void Awake() 
        {
            _character = GetComponent<Character>();
            foreach (AbilityBase ability in Abilities)
                ability.Initialize(_character);
        }

        public void SetCombatState(CombatState newCombatState)
        {
            CombatState oldCombatState = newCombatState;
            CombatState = newCombatState;
            OnCombatStateChanged.Invoke(oldCombatState, newCombatState);
        }

        [Button]
        public void AttemptToFireAbility(int abilityIndex) 
        {
            if (CombatState != CombatState.None) return;

            Abilities[abilityIndex].BeginCastingAbility();
            SetCombatState(CombatState.Attacking);
        }

        internal virtual Vector3 GetAimPosition()
        {
            return transform.forward;
        }
        


        
        public void TakeDamage(float damage)
        {
            CurrentHealth -= damage;
            Debug.Log($"{this.name} Took {damage} Damage");
            if (CurrentHealth < 0) Die();
        }

        private void Die()
        {
            Debug.Log($"{this.name} Died");
        }

    }
}
