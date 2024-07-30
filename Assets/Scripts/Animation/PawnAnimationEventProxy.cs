using UnityEngine;

public class PawnAnimationEventProxy : MonoBehaviour
{
    public delegate void AnimEventDelegate();

    public delegate void AnimEventDelegateParam(string animationName);

    public AnimEventDelegateParam OnAnimationStartListener;

    public AnimEventDelegateParam OnAnimationEndListener;

    public AnimEventDelegate OnKillMoveCameraShakeListener;

    private void OnAnimationStart(string a_AnimationName)
    {
        if (OnAnimationStartListener != null)
        {
            OnAnimationStartListener(a_AnimationName);
        }
    }

    private void OnAnimationEnd(string a_AnimationName)
    {
        if (OnAnimationEndListener != null)
        {
            OnAnimationEndListener(a_AnimationName);
        }
    }

    private void OnKillMoveCameraShake()
    {
        if (OnKillMoveCameraShakeListener != null)
        {
            OnKillMoveCameraShakeListener();
        }
    }
}
