using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private class MusicChannel
    {
        public int[] MusicSourceIndices;

        public int ActiveMusicSourceIndex;
    }

    private AudioSource sfxSource;

    private AudioListener listener;

    private List<AudioClip> delayedAudioClips = new List<AudioClip>();

    private List<float> delayedAudioClipsVolumes = new List<float>();

    private List<float> delayedAudioClipsTimers = new List<float>();

    public int MusicChannelCount = 3;

    private List<MusicChannel> musicChannels = new List<MusicChannel>();

    private AudioSource[] musicSources;

    private float[] musicSourcesTargetVolume;

    public float TransitionTime = 2f;

    private bool musicEnabled = true;

    private bool soundEnabled = true;

    private Dictionary<List<AudioClip>, SlowMorphRandom> randomizers = new Dictionary<List<AudioClip>, SlowMorphRandom>();

    private int MusicSourceCount
    {
        get
        {
            return 2 * MusicChannelCount;
        }
    }
    public bool MusicEnabled => musicEnabled;
    public bool SoundEnabled => soundEnabled;

    private void Awake()
    {
        listener = GetComponent<AudioListener>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.mute = !soundEnabled;

        musicSources = new AudioSource[MusicSourceCount];
        musicSourcesTargetVolume = new float[MusicSourceCount];

        for (int i = 0; i < MusicSourceCount; i++)
        {
            musicSources[i] = gameObject.AddComponent<AudioSource>();
            musicSources[i].loop = true;
            musicSources[i].mute = !MusicEnabled;
            musicSources[i].volume = 0f;
            musicSourcesTargetVolume[i] = 0f;
        }

        int num = 0;

        for (int j = 0; j < MusicChannelCount; j++)
        {
            MusicChannel musicChannel = new MusicChannel();
            musicChannel.MusicSourceIndices = new int[2];
            musicChannel.MusicSourceIndices[0] = num;
            musicChannel.MusicSourceIndices[1] = num + 1;
            musicChannel.ActiveMusicSourceIndex = num;
            musicChannels.Add(musicChannel);
            num += 2;
        }
    }
    private void Update()
    {
        for (int num = delayedAudioClips.Count - 1; num >= 0; num--)
        {
            AudioClip audioClip = delayedAudioClips[num];
            float volume = delayedAudioClipsVolumes[num];

            if (Time.time > delayedAudioClipsTimers[num])
            {
                delayedAudioClips.RemoveAt(num);
                delayedAudioClipsTimers.RemoveAt(num);
                delayedAudioClipsVolumes.RemoveAt(num);
                PlaySoundOnce(audioClip, volume);
            }
        }
        UpdateMusicFade();
    }
    public void ToggleMusic()
    {
        ToggleMusic(true);
    }

    private void ToggleMusic(bool writeToPref)
    {
        musicEnabled = !MusicEnabled;
      
        for (int i = 0; i < MusicSourceCount; i++)
        {
            musicSources[i].mute = !MusicEnabled;
        }
    }

    public void ToggleSound()
    {
        ToggleSound(true);
    }

    private void ToggleSound(bool writeToPref)
    {
        soundEnabled = !soundEnabled;
     
        sfxSource.mute = !soundEnabled;
    }

    public void FadeIn(int channel, AudioClip music, bool restartMusic)
    {
        if (music == null)
        {
            return;
        }

        MusicChannel musicChannel = musicChannels[channel];
        AudioSource audioSource = musicSources[musicChannel.ActiveMusicSourceIndex];

        if (restartMusic || audioSource.clip != music)
        {
            musicSourcesTargetVolume[musicChannel.ActiveMusicSourceIndex] = 0f;
            musicChannel.ActiveMusicSourceIndex = ((musicChannel.ActiveMusicSourceIndex != musicChannel.MusicSourceIndices[0]) ? musicChannel.MusicSourceIndices[0] : musicChannel.MusicSourceIndices[1]);
            audioSource = musicSources[musicChannel.ActiveMusicSourceIndex];
            if (audioSource.clip != music)
            {
                audioSource.clip = music;
                audioSource.Play();
                audioSource.volume = 0f;
            }
            else if (restartMusic)
            {
                audioSource.Play();
            }
            musicSourcesTargetVolume[musicChannel.ActiveMusicSourceIndex] = 1f;
        }
    }

    public void FadeOut(int channel)
    {
        MusicChannel musicChannel = musicChannels[channel];
        AudioSource audioSource = musicSources[musicChannel.ActiveMusicSourceIndex];
        if (audioSource.clip != null)
        {
            musicSourcesTargetVolume[musicChannel.ActiveMusicSourceIndex] = 0f;
        }
    }

    private void UpdateMusicFade()
    {
        for (int i = 0; i < MusicSourceCount; i++)
        {
            AudioSource audioSource = musicSources[i];
            float volume = musicSourcesTargetVolume[i];

            if (audioSource.volume == volume)
            {
                continue;
            }

            if (musicSourcesTargetVolume[i] > audioSource.volume)
            {
                float newVolume = audioSource.volume + 1f / TransitionTime * Time.deltaTime;

                if (newVolume >= 1f)
                {
                    newVolume = 1f;
                }
                audioSource.volume = newVolume;
            }

            else if (musicSourcesTargetVolume[i] < audioSource.volume)
            {
                float newVolume = audioSource.volume - 1f / TransitionTime * Time.deltaTime;

                if (newVolume <= 0f)
                {
                    newVolume = 0f;
                    audioSource.clip = null;
                }

                audioSource.volume = newVolume;
            }
        }
    }

    public void PlaySoundOnceAmong(List<AudioClip> sounds, float volume)
    {
        if (sounds.Count == 0)
        {
            return;
        }
        if (sounds.Count == 1)
        {
            PlaySoundOnce(sounds[0], volume);
            return;
        }

        SlowMorphRandom value = null;

        if (!randomizers.TryGetValue(sounds, out value))
        {
            value = new SlowMorphRandom(0, sounds.Count);
            randomizers[sounds] = value;
        }

        int next = value.Next;

        PlaySoundOnce(sounds[next], volume);
    }

    public void PlaySoundOnceAmongDelayed(List<AudioClip> sounds, float volume, float delay)
    {
        if (sounds.Count == 0)
        {
            return;
        }
        if (sounds.Count == 1)
        {
            PlaySoundOnce(sounds[0], volume);
            return;
        }
        SlowMorphRandom value = null;
        if (!randomizers.TryGetValue(sounds, out value))
        {
            value = new SlowMorphRandom(0, sounds.Count);
            randomizers[sounds] = value;
        }
        int next = value.Next;
        PlaySoundOnceDelayed(sounds[next], volume, delay);
    }

    public void PlaySoundOnce(AudioClip audioClip, float volume)
    {
        if (!(audioClip == null) && listener.enabled)
        {
            sfxSource.PlayOneShot(audioClip, Mathf.Pow(10f, volume / 20f));
        }
    }

    public void PlaySoundOnceDelayed(AudioClip audioClip, float volume, float delay)
    {
        if (!(audioClip == null) && listener.enabled)
        {
            delayedAudioClips.Add(audioClip);
            delayedAudioClipsVolumes.Add(volume);
            delayedAudioClipsTimers.Add(Time.time + delay);
        }
    }

}
