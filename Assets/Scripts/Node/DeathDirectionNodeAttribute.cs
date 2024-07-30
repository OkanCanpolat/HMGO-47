using UnityEngine;

public class DeathDirectionNodeAttribute : NodeAttribute
{
    protected override void Start()
    {
        MeshRenderer componentInChildren = GetComponentInChildren<MeshRenderer>();
        if ((bool)componentInChildren)
        {
            Destroy(componentInChildren);
        }
    }
  
}
