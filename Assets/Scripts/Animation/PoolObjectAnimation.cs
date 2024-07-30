using UnityEngine;

public class PoolObjectAnimation : MonoBehaviour
{
    public float AnimationSpeed = 1f;

    private float AnimationCounter;

    private Transform Transform;

    public float Amplitude = 1f;

    private Vector3 StartPos;

    private void Start()
    {
        Transform = transform;
        StartPos = Transform.position;
        AnimationCounter = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        AnimationCounter = Mathf.Repeat(AnimationCounter + Time.deltaTime, Mathf.PI * 2f);
        Vector3 zero = Vector3.zero;
        zero.y = Mathf.Sin(AnimationCounter * AnimationSpeed * 1f) * 0.05f;
        Transform.position = StartPos + zero;
        float x = Amplitude * Mathf.Sin(AnimationCounter * AnimationSpeed * 0.5f);
        float z = Amplitude * Mathf.Sin(AnimationCounter * AnimationSpeed * 1f);
        Transform.rotation = Quaternion.Euler(x, Transform.localEulerAngles.y, z);
    }
}
