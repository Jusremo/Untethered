using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using Sirenix.OdinInspector;

public enum Layers {Characters = 6}
public enum LayerMasks 
{ 
    Characters = (1 << Layers.Characters) 
}

namespace Untethered.Utility
{
    [System.Serializable]
    public class RangeVector3
    {
        [OnStateUpdate("UpdateVector3")]
        [SerializeField, Range(-1, 1), HorizontalGroup("Group 1", LabelWidth = 20)] public float x , y, z;

        [HideInInspector] public Vector3 Vector3;

        public void UpdateVector3()
        {
            Vector3.x = x;
            Vector3.y = y;
            Vector3.z = z;
        }
    }
    public class MathUtility 
    {
        public static enumType RandomEnumValue<enumType>()
        {
            var values = Enum.GetValues(typeof(enumType));
            int random = UnityEngine.Random.Range(0, values.Length);
            return (enumType)values.GetValue(random);
        }

        public static Vector3 RotateAroundPoint(Vector3 position, Vector3 centerPoint, Vector3 axis, float angle)
        {
            Vector3 newPoint = Quaternion.AngleAxis(angle, axis) * (position - centerPoint);
            Vector3 resultVec3 = centerPoint + newPoint;
            return resultVec3;
        }

        public static float AngleDir(Vector3 centerDir, Vector3 dirToCompare, Vector3 up) 
		{
			Vector3 perp = Vector3.Cross(centerDir, dirToCompare);
			float dir = Vector3.Dot(perp, up);
			
			if (dir > 0f) {
				return 1f;
			} else if (dir < 0f) {
				return -1f;
			} else {
				return 0f;
			}
		}
    }
}

[System.Serializable] public struct MinMaxRange
{
    [SerializeField] private float _min, _max;
    public MinMaxRange(float min, float max)
    {
        _min = min;
        _max = max;
    }

    public float Min { get => _min; }
    public float Max { get => _max; }
}

