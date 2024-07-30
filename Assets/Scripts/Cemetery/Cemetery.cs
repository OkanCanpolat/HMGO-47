using System.Collections.Generic;
using UnityEngine;

public class Cemetery : MonoBehaviour
{
    [HideInInspector]
    public List<Pawn> DeadPawns = new List<Pawn>();

    public Vector3 Offset = Vector3.left;

    private void Start()
    {
        if (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
        }
    }
}
