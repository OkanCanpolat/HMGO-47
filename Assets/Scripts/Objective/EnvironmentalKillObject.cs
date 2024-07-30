using UnityEngine;
using Zenject;

public class EnvironmentalKillObject : MonoBehaviour
{
    public Vector3 cameraShakeAmount = new Vector3(0.2f, 0f, 0.2f);

    public float cameraShakeDuration = 0.3f;

    public EnvironmentalKillSoundConfig soundConfig;

    [HideInInspector] public EnvironmentalKillNodeAttribute TiedNodeAttribute;

    private Animator m_Animator;
    [Inject] private GameManager gameManager;
    [Inject] private AudioManager audioManager;
    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    public void Trigger()
    {
        m_Animator.SetTrigger("Trigger");
    }

    public void OnCameraShake()
    {
        gameManager.ShakeCamera(cameraShakeAmount, cameraShakeDuration);
    }

    public void OnEnvironmentalKill()
    {
        TiedNodeAttribute.OnEnvironmentalKill();
    }

    public void OnTimeScale(float scale)
    {
        Time.timeScale = scale;
    }

    public void OnAnimationEnd()
    {
        TiedNodeAttribute.OnAnimationEnd();
    }

    public void OnTriggerSound()
    {
        if (soundConfig != null)
        {
            audioManager.PlaySoundOnce(soundConfig.StatueFallSound, soundConfig.StatueFallSoundVolume);
        }
    }
}
