using UnityEngine;

public class SecretPassageDoorAnimation : SecretPassageAnimation
{
    private Quaternion startRotation = Quaternion.identity;

    private void Start()
    {
        startRotation = transform.rotation;
    }

    public override void Open(float duration, float delay)
    {
        Quaternion quaternion = startRotation * Quaternion.Euler(Vector3.left * 0.25f * Mathf.Sign(transform.localScale.z) * 360f);
        iTween.RotateTo(gameObject, iTween.Hash("rotation", quaternion.eulerAngles, "time", duration, "delay", delay, "easetype", iTween.EaseType.easeOutBounce));
    }

    public override void Close(float a_Duration, float a_Delay)
    {
        iTween.RotateTo(gameObject, iTween.Hash("rotation", startRotation.eulerAngles, "time", a_Duration, "delay", a_Delay, "easetype", iTween.EaseType.easeInCubic));
    }
}
