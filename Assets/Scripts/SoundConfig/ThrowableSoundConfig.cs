using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "PlayerPawnSoundConfig", menuName = "Sounds/PlayerPawnSoundConfig")]

public class ThrowableSoundConfig : ScriptableObject
{
    public List<AudioClip> PickupSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float PickupVolume = 1f;

    public List<AudioClip> LandingSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float LandingVolume = 1f;

    public List<AudioClip> ThrowSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float ThrowVolume = 1f;

    public List<AudioClip> RadiusSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float RadiusVolume = 1f;
}
