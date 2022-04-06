using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Events;

namespace Untethered.Characters
{
    public class PlayerCombat : Combat
    {
        internal override Vector3 GetAimPosition()
        {
            return ((Player)_character).Camera.forward;
        }
    }
}
