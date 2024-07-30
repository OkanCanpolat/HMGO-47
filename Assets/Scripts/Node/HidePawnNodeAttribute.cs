using UnityEngine;
using Zenject;

public class HidePawnNodeAttribute : NodeAttribute
{
    public GameObject FlowerPot;

    public HideNodeSoundConfig SoundConfig;

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
            MeshRenderer[] componentsInChildren = pawn.transform.GetChild(0).gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in componentsInChildren)
            {
                meshRenderer.enabled = false;
            }
            gameManager.HasHidingSpotBeenUsed = true;
            iTween.PunchRotation(FlowerPot, iTween.Hash("amount", 25f * Vector3.forward, "time", 1f));
            audioManager.PlaySoundOnceAmong(SoundConfig.ArrivalSounds, SoundConfig.ArrivalVolume);
        }
    }

    public override void OnPawnDeparture(Pawn pawn, Barrier barrier)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        if (pawn is PlayerPawn)
        {
            barrier.Add(this);
            MeshRenderer[] componentsInChildren = pawn.transform.GetChild(0).gameObject.GetComponentsInChildren<MeshRenderer>(true);
            MeshRenderer[] array = componentsInChildren;
            foreach (MeshRenderer meshRenderer in array)
            {
                meshRenderer.enabled = true;
            }
            iTween.PunchRotation(FlowerPot, iTween.Hash("amount", 25f * Vector3.forward, "time", 1f));
            barrier.RemoveIn(this, 0.5f);
            audioManager.PlaySoundOnceAmong(SoundConfig.DepartureSounds, SoundConfig.DepartureVolume);
        }
        base.OnPawnDeparture(pawn, barrier);
    }
}
