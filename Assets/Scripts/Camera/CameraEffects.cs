using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    private CameraSphericalMovement cameraSphericalMovement;

    private void Start()
    {
        cameraSphericalMovement = GetComponent<CameraSphericalMovement>();
    }

    public void Shake(Vector3 amount, float duration)
    {
        if (!(cameraSphericalMovement != null) || !cameraSphericalMovement.IsInAnimation())
        {
          iTween.ShakePosition(gameObject, amount, duration);
        }
    }
}
