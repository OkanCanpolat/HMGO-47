using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu (fileName = "AiPawnSoundConfig", menuName = "Sounds/Ai PawnSound Config")]
public class AiPawnSoundConfig : ScriptableObject
{
    public List<AudioClip> SingleMoveSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float SingleMoveVolume = 1f;

    public List<AudioClip> DoubleMoveSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float DoubleMoveVolume = 1f;

    public List<AudioClip> TripleMoveSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float TripleMoveVolume = 1f;

    public List<AudioClip> SingleRotateSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float SingleRotateVolume = 1f;

    public List<AudioClip> DoubleRotateSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float DoubleRotateVolume = 1f;

    public List<AudioClip> TripleRotateSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float TripleRotateVolume = 1f;

    public List<AudioClip> KillSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float KillVolume = 1f;

    public List<AudioClip> OpenQuestionSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float OpenQuestionVolume = 1f;

    public List<AudioClip> CloseQuestionSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float CloseQuestionVolume = 1f;

    public List<AudioClip> DogExclamationSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float DogExclamationVolume = 1f;
}
