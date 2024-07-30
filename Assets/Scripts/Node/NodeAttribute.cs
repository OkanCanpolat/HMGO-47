using UnityEngine;
using Zenject;

public class NodeAttribute : MonoBehaviour
{
    protected Node currentNode;

    protected bool isActive = true;

    public bool ShakeOnArrival;

    public float ShakeAnimDuration = 0.1f;

    public float ShakeMaxAngle = 25f;

    public int ShakeCount = 2;

    private int shakeCurrentCount;

    protected FaceToCamera playerPawn;

    private Vector3 startEular;

    private Barrier nodeBarrier;

    [Inject] protected GameManager gameManager;
    public bool IsActive => isActive;
    protected virtual void Start()
    {
        currentNode = GetComponentInParent<Node>();
    }

    public virtual void OnEnterNodeSelection(bool shouldSelectNodeForInteraction)
    {
    }
    public virtual void OnLeaveNodeSelection()
    {
    }
    public virtual void OnBeginningOfPlayerTurn()
    {
    }
    public virtual void OnPlayerPawnKilled()
    {
    }
    public virtual void OnPawnDeparture(Pawn pawn, Barrier barrier)
    {
    }
    public virtual void OnPlayerStartInteract(Barrier barrier)
    {
    }
    public virtual void OnPlayerInteract(Node selectedNode, Barrier barrier)
    {
    }
    public virtual void OnPlayerSniperShot(Barrier barrier)
    {
    }
    public virtual void OnPawnArrival(Pawn pawn, Barrier barrier)
    {
        if (gameObject.activeInHierarchy && isActive)
        {
            PlayerPawn playerPawn = pawn as PlayerPawn;
            if ((bool)playerPawn && ShakeOnArrival)
            {
                StartShakeAnim(barrier);
            }
        }
    }
    protected void StartShakeAnim(Barrier a_Barrier)
    {
        playerPawn = gameManager.PlayerPawn.GetComponentInChildren<FaceToCamera>();
        playerPawn.enabled = false;
        StartShakeRotation();
        nodeBarrier = a_Barrier;
        nodeBarrier.Add(this);
        shakeCurrentCount = 0;
    }
    private void StartShakeRotation()
    {
        float shakeMaxAngle = ShakeMaxAngle;
        startEular = playerPawn.transform.eulerAngles;
        playerPawn.transform.eulerAngles += new Vector3(0f, 0f, 0f - shakeMaxAngle);
        iTween.RotateTo(playerPawn.gameObject, iTween.Hash("time", ShakeAnimDuration, "rotation", startEular + new Vector3(0f, 0f, shakeMaxAngle), "easetype", iTween.EaseType.linear, "looptype", "pingPong", "onComplete", "ShakeComplete", "onCompleteTarget", gameObject));
    }
    protected void HideAllMeshes()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        NodeItem[] nodeItems = GetComponentsInChildren<NodeItem>();

        foreach (NodeItem nodeItem in nodeItems)
        {
            nodeItem.gameObject.SetActive(false);
        }
    }
}
