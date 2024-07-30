using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "KeyDoorSoundConfig", menuName = "Sounds/Key Door")]
[Serializable]
public class KeyDoorSoundConfig : ScriptableObject
{
    public List<AudioClip> PickupSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float PickupSoundsVolume = 1f;
}
