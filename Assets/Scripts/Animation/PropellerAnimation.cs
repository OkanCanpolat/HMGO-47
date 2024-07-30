using UnityEngine;

public class PropellerAnimation : MonoBehaviour
{
    public float Frequency = 1f;

    private void Update()
    {
        Quaternion localRotation = Quaternion.Euler(new Vector3(0f, 0f, 360f * Frequency * Time.time));
        transform.localRotation = localRotation;
    }
}
