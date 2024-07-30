using UnityEngine;

public class GardenerMovement : MonoBehaviour
{
    private void Start()
    {
        iTween.ShakeRotation(gameObject, iTween.Hash("x", 0.8f, "time", 1f, "looptype", "loop"));
    }
}
