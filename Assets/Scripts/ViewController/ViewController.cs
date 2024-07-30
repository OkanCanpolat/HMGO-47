using UnityEngine;

public class ViewController : MonoBehaviour
{
    [HideInInspector]
    public ViewManager ViewManager;

    public virtual void OnEnter()
    {
    }
    public virtual void OnLeave()
    {
    }
    public virtual void OnBackPressed()
    {
    }
    public virtual void OnPlayerMovingToEndNode()
    {
    }
}
