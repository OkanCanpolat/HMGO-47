using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "UiSoundConfig", menuName = "Sounds/Ui Sound Config")]
public class UiSoundConfig : ScriptableObject
{
    public AudioClip IntroSound;

    [Range(-100f, 0f)]
    public float IntroVolume = 1f;

    public AudioClip OutroSound;

    [Range(-100f, 0f)]
    public float OutroVolume = 1f;

    public AudioClip LevelCompleteSound;

    [Range(-100f, 0f)]
    public float LevelCompleteVolume = 1f;

    public List<AudioClip> LevelResultsStampSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float LevelResultsStampVolume = 1f;

    public List<AudioClip> LevelResultsCardSlideSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float LevelResultsCardSlideVolume = 1f;
}
