using System;
using UnityEngine;

namespace Player.Looking
{
    [Serializable]
    public class CameraBobSetting
    {
        [Range(0f, 5f)] public float frequency = 1f;

        [Range(0, 5f)] public float maxSpeed = 5f;

        [Range(0, 5f)] public float horizontalAmplitude;

        [Range(0, 5f)] public float verticalAmplitude;

        [Range(0, 50f)] public float speedCurve = 0.00001f;
    }
}