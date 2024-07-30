using UnityEngine;

public class TreeAnimation : MonoBehaviour
{
    private float animationSpeed = 1f;

    private float animationCounter;

    private void Start()
    {
        animationCounter = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        animationCounter = Mathf.Repeat(animationCounter + Time.deltaTime, Mathf.PI * 2f);
        Vector3 localEulerAngles = transform.localEulerAngles;
        localEulerAngles.z = Mathf.Sin(animationCounter * animationSpeed);
        transform.localEulerAngles = localEulerAngles;
    }
}
