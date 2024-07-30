using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "SecretPaasageSoundConfig", menuName = "Sounds/Secret Paasage")]
public class SecretPaasageSoundConfig : ScriptableObject
{
    public List<AudioClip> PassageSounds = new List<AudioClip>();

    [Range(-100f, 0f)]
    public float PassageVolume = 1f;
}
