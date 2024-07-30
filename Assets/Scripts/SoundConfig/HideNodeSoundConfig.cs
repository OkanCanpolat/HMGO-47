using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "HideNodeSoundConfig", menuName = "Sounds/HideNodeSoundConfig")]

public class HideNodeSoundConfig : ScriptableObject
{
    public List<AudioClip> ArrivalSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float ArrivalVolume = 1f;

    public List<AudioClip> DepartureSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float DepartureVolume = 1f;
}
