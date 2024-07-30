using UnityEngine;

public class ExtractionPointAnimation : MonoBehaviour
{
    private Transform objecTtransform;

    private void Start()
    {
        objecTtransform = transform;
    }

    private void Update()
    {
        objecTtransform.Rotate(Vector3.up * Time.deltaTime * 10f);
    }
}
