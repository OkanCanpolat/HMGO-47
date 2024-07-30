using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
[CreateAssetMenu(fileName = "SilverBallerSoundConfig", menuName = "Sounds/SilverBallerSoundConfig")]

public class SilverBallerSoundConfig : ScriptableObject
{
    public List<AudioClip> PickupSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float PickupVolume = 1f;

    public List<AudioClip> ShootSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float ShootVolume = 1f;

    public List<AudioClip> ReboundSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float ReboundVolume = 1f;
}
