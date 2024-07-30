using UnityEngine;
using Zenject;

public class DisguiseNodeAttribute : NodeAttribute
{
    public Material Material;

    public PawnColor Color;

    public DisguiseSoundConfig SoundConfig;

    [Inject] private AudioManager audioManager;

    public override void OnPawnArrival(Pawn pawn, Barrier barrier)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        base.OnPawnArrival(pawn, barrier);

        if (isActive)
        {
            PlayerPawn playerPawn = pawn as PlayerPawn;

            if ((bool)playerPawn)
            {
                audioManager.PlaySoundOnceAmong(SoundConfig.PickupSounds, SoundConfig.PickupVolume);
                isActive = false;
                playerPawn.SetDisguise(Material, Color);
                HideAllMeshes();
            }
        }
    }
}
