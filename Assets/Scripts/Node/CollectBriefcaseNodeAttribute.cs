
using Zenject;

public class CollectBriefcaseNodeAttribute : NodeAttribute
{
    public BriefcaseSoundConfig SoundConfig;
    private bool isCollected;
    [Inject] private AudioManager audioManager;

    public override void OnPawnArrival(Pawn pawn, Barrier barrier)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        base.OnPawnArrival(pawn, barrier);
        if (pawn is PlayerPawn)
        {
            if (!isCollected)
            {
                gameManager.CollectedBriefcases.Add(gameObject);
                audioManager.PlaySoundOnceAmong(SoundConfig.PickupSounds, SoundConfig.PickupVolume);
            }
            isCollected = true;
            gameObject.SetActive(false);
        }
    }
}
