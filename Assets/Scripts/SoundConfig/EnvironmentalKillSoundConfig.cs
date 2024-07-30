using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "EnvironmentalKillSoundConfig", menuName = "Sounds/EnvironmentalKillSoundConfig")]

public class EnvironmentalKillSoundConfig : ScriptableObject
{
    public AudioClip StatueFallSound;

    [Range(-100f, 0f)]
    public float StatueFallSoundVolume = 1f;
}
