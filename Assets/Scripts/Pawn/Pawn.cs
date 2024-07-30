using System;
using UnityEngine;
public enum PawnColor
{
    Yellow, Blue, Red, Grey, Purple, Black, White, Brown, Green, Orange
}
public enum Orientation
{
    Invalid, PositiveZ, PositiveX, NegativeZ, NegativeX
}

public class Pawn : MonoBehaviour
{
    public PawnColor CurrentColor;
    public float DistanceFromCenter = 0.75f;
    public float MoveHopsFrequency = 1f;
    public float MoveHopsHeight = 1f;
    public float MoveBalanceAmplitude = 10f;
    public float MoveTime = 0.5f;
    public float MoveOffsetTime = 0.5f;
    public float RotateTime = 0.25f;
    public Interpolation.EaseType RotateEaseType = Interpolation.EaseType.linear;
    public Interpolation.EaseType MoveEaseType = Interpolation.EaseType.linear;
    protected FSM fsm = new FSM();
    public bool IsFsmDebugActivated;

    private Node lastNode;
    private Node currentNode;
    private Node targetNode;
    protected Barrier barrier = new Barrier();
    private Orientation currentOrientation;
    private Orientation targetOrientation;

    protected bool isPassive;
    protected Animator animator;
    protected Transform meshTransform;

    public Node LastNode => lastNode;
    public Node CurrentNode => currentNode;
    public Node TargetNode => targetNode;
    public Orientation CurrentOrientation => currentOrientation;
    public Orientation TargetOrientation => targetOrientation;
    public bool IsPassive { get => isPassive; set => isPassive = value; }
    public Transform MeshTransform
    {
        get
        {
            if (meshTransform == null)
            {
                Transform[] componentsInChildren = GetComponentsInChildren<Transform>(true);

                foreach (Transform transform in componentsInChildren)
                {
                    if (transform.gameObject.name == "Mesh")
                    {
                        meshTransform = transform;
                    }
                }
            }
            return meshTransform;
        }
    }
    protected virtual void Awake()
    {
        animator = gameObject.GetComponentInChildren<Animator>();
        RegisterPawnAnimationEventProxy();
    }
    protected virtual void OnDestroy()
    {
        UnregisterPawnAnimationEventProxy();
    }
    public void RequestState(FSM.StateDelegate requestedState)
    {
        fsm.RequestState(requestedState);
    }
    public bool IsOnPushingNode()
    {
        if (CurrentNode != null && CurrentNode.GetNodeAttribute<PushPawnsNodeAttribute>() != null)
        {
            return true;
        }
        return false;
    }
    public bool WasOnPushingNode()
    {
        if (LastNode != null && LastNode.GetNodeAttribute<PushPawnsNodeAttribute>() != null)
        {
            return true;
        }
        return false;
    }
    public void UpdateFsm()
    {
        fsm.SetDebugActivation(IsFsmDebugActivated);
        fsm.Update();
    }
    public virtual bool IsPawnValidTarget(Pawn pawn)
    {
        Node currentNode = pawn.CurrentNode;

        DisguiseNodeAttribute nodeAttribute = currentNode.GetNodeAttribute<DisguiseNodeAttribute>();
        
        if (!pawn.IsDead() && (pawn.CurrentColor != CurrentColor || (nodeAttribute != null && nodeAttribute.IsActive && nodeAttribute.Color != pawn.CurrentColor)) && currentNode.GetNodeAttribute<HidePawnNodeAttribute>() == null)
        {
            return true;
        }
        return false;
    }
    public void SetCurrentNode(Node node)
    {
        if(node == null) 

        if (currentNode != null)
        {
            currentNode.DepartingPawns.Remove(this);
        }
        if (targetNode != null)
        {
            targetNode.ArrivingPawns.Remove(this);
        }
        if (currentNode != node)
        {
            lastNode = currentNode;
            currentNode = node;

            if (node != null)
            {
               currentNode.Pawns.Add(this);
            }
        }

        targetNode = node;
    }
    public void SetTargetNode(Node node)
    {
        targetNode = node;

        if (node != currentNode)
        {
            if ((bool)currentNode)
            {
                currentNode.Pawns.Remove(this);
                currentNode.DepartingPawns.Add(this);
            }
            if ((bool)node)
            {
                targetNode.ArrivingPawns.Add(this);
            }
        }
    }
    public virtual void OnSpawned()
    {
    }
    public virtual bool IsDead()
    {
        return false;
    }
    public void SetTargetOrientation(Orientation orientation)
    {
        targetOrientation = orientation;
    }
    public void SetCurrentOrientation(Orientation orientation)
    {
        currentOrientation = orientation;
    }
    protected void RegisterPawnAnimationEventProxy()
    {
        PawnAnimationEventProxy componentInChildren = gameObject.GetComponentInChildren<PawnAnimationEventProxy>();

        if (componentInChildren != null)
        {
            componentInChildren.OnAnimationStartListener = (PawnAnimationEventProxy.AnimEventDelegateParam)Delegate.Combine(componentInChildren.OnAnimationStartListener, new PawnAnimationEventProxy.AnimEventDelegateParam(OnAnimationStart));
            componentInChildren.OnAnimationEndListener = (PawnAnimationEventProxy.AnimEventDelegateParam)Delegate.Combine(componentInChildren.OnAnimationEndListener, new PawnAnimationEventProxy.AnimEventDelegateParam(OnAnimationEnd));
            componentInChildren.OnKillMoveCameraShakeListener = (PawnAnimationEventProxy.AnimEventDelegate)Delegate.Combine(componentInChildren.OnKillMoveCameraShakeListener, new PawnAnimationEventProxy.AnimEventDelegate(OnKillMoveCameraShake));
        }
    }

    protected void UnregisterPawnAnimationEventProxy()
    {
        PawnAnimationEventProxy componentInChildren = gameObject.GetComponentInChildren<PawnAnimationEventProxy>();

        if (componentInChildren != null)
        {
            componentInChildren.OnAnimationStartListener = (PawnAnimationEventProxy.AnimEventDelegateParam)Delegate.Remove(componentInChildren.OnAnimationStartListener, new PawnAnimationEventProxy.AnimEventDelegateParam(OnAnimationStart));
            componentInChildren.OnAnimationEndListener = (PawnAnimationEventProxy.AnimEventDelegateParam)Delegate.Remove(componentInChildren.OnAnimationEndListener, new PawnAnimationEventProxy.AnimEventDelegateParam(OnAnimationEnd));
            componentInChildren.OnKillMoveCameraShakeListener = (PawnAnimationEventProxy.AnimEventDelegate)Delegate.Remove(componentInChildren.OnKillMoveCameraShakeListener, new PawnAnimationEventProxy.AnimEventDelegate(OnKillMoveCameraShake));
        }
    }

    protected virtual void OnAnimationStart(string animationName)
    {
    }

    protected virtual void OnAnimationEnd(string animationName)
    {
    }

    protected virtual void OnKillMoveCameraShake()
    {
    }
    public virtual void OnAiPawnKilled(AiPawn aiPawn)
    {
        if (isPassive)
        {
            isPassive = false;
        }
    }

    public virtual void OnPlayerBreakPassive()
    {
        if (isPassive)
        {
            isPassive = false;
        }
    }
}
