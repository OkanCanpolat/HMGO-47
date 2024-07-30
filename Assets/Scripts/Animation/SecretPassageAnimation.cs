using UnityEngine;

public class SecretPassageAnimation : MonoBehaviour
{
    private Vector3 position = Vector3.zero;

    private void Fixup()
    {
        transform.position = position;
    }

    public virtual void Open(float duration, float delay)
    {
        position = transform.position;
        BaseAnimation animation = new MoveAddAnimation(duration, Vector3.left * 1.2f, Interpolation.EaseType.easeOutExpo);
        Animateur.PushAnimation(gameObject, animation);
    }

    public virtual void Close(float duration, float delay)
    {
        BaseAnimation baseAnimation = new MoveAddAnimation(duration, -Vector3.left * 1.2f, Interpolation.EaseType.easeOutExpo);
        baseAnimation.Delay = delay;
        baseAnimation.AnimationFinishedDelegate = Fixup;
        Animateur.PushAnimation(gameObject, baseAnimation);
    }
}
