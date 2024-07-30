using UnityEngine;

public class FaceToCamera : MonoBehaviour
{
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }
    private void Update()
    {
        FaceCamera();
    }
    public void FaceCamera()
    {
        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 eulerAngles = Quaternion.LookRotation(cameraPos - transform.position, Vector3.up).eulerAngles;
        Vector3 currentRotation = transform.rotation.eulerAngles;
        transform.eulerAngles = new Vector3(currentRotation.x, eulerAngles.y, currentRotation.z);
        transform.localEulerAngles = new Vector3(0f, transform.localEulerAngles.y, 0f);
    }
}
