using UnityEngine;

public class IndicatorAnimation : MonoBehaviour
{
    protected bool shownAsHint;

    protected Node currentNode;

    public virtual void StartAnimation()
    {
        currentNode = transform.parent.GetComponent<Node>();
    }

    protected virtual void UpdateIndicatorMaterial()
    {
    }
}
