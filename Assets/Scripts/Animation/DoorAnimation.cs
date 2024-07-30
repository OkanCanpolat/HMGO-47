using UnityEngine;

public class DoorAnimation : MonoBehaviour
{
    public float RotationAmount = 0.3f;

    public bool UsesAuthoredAnimation;

    private Animator Animator;

    public void Awake()
    {
        if (UsesAuthoredAnimation)
        {
            Animator = GetComponent<Animator>();
        }
    }

    public void OpenDoor()
    {
        if (Animator != null)
        {
            Animator.SetTrigger("DoorOpen");
            return;
        }
        iTween.RotateBy(gameObject, iTween.Hash("amount", Vector3.up * RotationAmount * Mathf.Sign(base.transform.localScale.z), "time", 0.6f, "easetype", "easeOutBounce"));
    }

    public void CloseDoor()
    {
        if (Animator != null)
        {
            Animator.SetTrigger("DoorClose");
            return;
        }
        iTween.RotateBy(gameObject, iTween.Hash("amount", -Vector3.up * RotationAmount * Mathf.Sign(base.transform.localScale.z), "time", 0.6f, "easetype", "easeOutBounce"));
    }
}
