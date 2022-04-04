using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Untethered.Utility;

namespace Untethered.Characters
{
    public class PlayerRigAimTarget : MonoBehaviour
    {
        private Player _player;

        private void Awake() 
        {
            _player = GetComponentInParent<Player>();
        }

        private void Update() => MoveToWhereCameraIsLooking();

        private void MoveToWhereCameraIsLooking()
        {
            Vector3 cameraPos = _player.Camera.position;
            Vector3 cameraForward =  _player.Camera.forward;

            transform.position = cameraPos + (cameraForward * 15);

        }
    }
}
