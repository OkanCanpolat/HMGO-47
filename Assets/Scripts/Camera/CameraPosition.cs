using UnityEngine;

public class CameraPosition : MonoBehaviour
{
    [HideInInspector]
    public float FieldOfView;

    [HideInInspector]
    public float NearClipPlane;

    [HideInInspector]
    public float FarClipPlane;

    private void Awake()
    {
        Camera component = GetComponent<Camera>();

        if (component != null)
        {
            FieldOfView = component.fieldOfView;
            NearClipPlane = component.nearClipPlane;
            FarClipPlane = component.farClipPlane;
            Destroy(component);
        }
    }
}
