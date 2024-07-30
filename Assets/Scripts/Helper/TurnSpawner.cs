using UnityEngine;

[ExecuteInEditMode]
public class TurnSpawner : MonoBehaviour
{
    [SerializeField] private Orientation orientation;

    public void Start()
    {
        if (Application.isPlaying)
        {
            MeshRenderer component = GetComponent<MeshRenderer>();

            if ((bool)component)
            {
                component.enabled = false;
            }
        }
    }

    public void Update()
    {
        Transform parent = transform.parent;

        if ((bool)parent)
        {
            SpawnAiNodeAttribute component = parent.GetComponent<SpawnAiNodeAttribute>();

            if ((bool)component && component.Orientation != orientation)
            {
                orientation = component.Orientation;
                transform.rotation = orientation.OrientationToQuaternion();
            }
        }
    }
}
