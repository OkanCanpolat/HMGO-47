using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "EnvironmentalKillSoundConfig", menuName = "Sounds/EnvironmentalKillSoundConfig")]

public class DisguiseSoundConfig : ScriptableObject
{
    public List<AudioClip> PickupSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float PickupVolume = 1f;
}
