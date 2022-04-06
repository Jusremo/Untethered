using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using System;
using UnityEngine.Events;

namespace Untethered.Characters
{
    
    public class OnParticleCollisionCallback : MonoBehaviour 
    {
        [HideInInspector] public UnityEvent<GameObject, Vector3> OnParticleCollisionEvent = new UnityEvent<GameObject, Vector3>();
        [SerializeField] private ParticleSystem _particleSystem;

        private List<ParticleCollisionEvent> _collisionEvents;

        private void Start() 
        {
            _collisionEvents = new List<ParticleCollisionEvent>();
        }

        public void OnParticleCollision(GameObject other)
        {
            ParticlePhysicsExtensions.GetCollisionEvents(_particleSystem, other, _collisionEvents);
    
            for (int i = 0; i < _collisionEvents.Count; i++)
                OnParticleCollisionEvent.Invoke(other, _collisionEvents[i].intersection);
        }
        
    }
}