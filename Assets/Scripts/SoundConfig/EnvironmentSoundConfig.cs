using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "EnvironmentSoundConfig", menuName = "Sounds/EnvironmentSoundConfig")]

public class EnvironmentSoundConfig : ScriptableObject
{
    public AudioClip NodeGraphRevealStartSound;

    [Range(-100f, 0f)]
    public float NodeGraphRevealStartVolume = 1f;
}
