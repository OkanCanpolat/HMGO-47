using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "PlayerPawnSoundConfig", menuName = "Sounds/PlayerPawnSoundConfig")]

public class PlayerPawnSoundConfig : ScriptableObject
{
    public List<AudioClip> NoKillMoveSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float NoKillMoveVolume = 1f;

    public List<AudioClip> SingleKillMoveSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float SingleKillMoveVolume = 1f;

    public List<AudioClip> DoubleKillMoveSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float DoubleKillMoveVolume = 1f;

    public List<AudioClip> TripleKillMoveSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float TripleKillMoveVolume = 1f;

    public List<AudioClip> NoKillManholeMoveSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float NoKillManholeMoveVolume = 1f;

    public List<AudioClip> SingleKillManholeMoveSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float SingleKillManholeMoveVolume = 1f;

    public List<AudioClip> DoubleKillManholeMoveSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float DoubleKillManholeMoveVolume = 1f;

    public List<AudioClip> TripleKillManholeMoveSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float TripleKillManholeMoveVolume = 1f;

    public AudioClip MarkKilledSound;

    [Range(-100f, 0f)]
    public float MarkKilledVolume = 1f;
}
