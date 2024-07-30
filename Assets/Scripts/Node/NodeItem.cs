using UnityEngine;
using Zenject;

public class NodeItem : MonoBehaviour
{
    public float MoveOffsetTime = 0.4f;
    private Node node;
    private FSM Fsm = new FSM();
    private bool isMoveAnimationFinished;
    private Vector3 targetPosition = Vector3.zero;
    [Inject] private GameManager gameManager;
    public void RequestState(FSM.StateDelegate requestedState)
    {
        Fsm.RequestState(requestedState);
    }

    private void Awake()
    {
        node = transform.parent.parent.GetComponent<Node>();

        Fsm.SetDebugName(gameObject.name);

        Fsm.Start(OnFirstFrameStateUpdate);
    }

    public void UpdateFsm()
    {
        Fsm.Update();
    }

    private FSM.StateDelegate OnFirstFrameStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (step == FSM.Step.Update)
        {
            transform.position = node.GetPositionForNodeItem();
            return OnIdleStateUpdate;
        }
        return null;
    }

    public FSM.StateDelegate OnIdleStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (step == FSM.Step.Transition)
        {
            return state;
        }
        return null;
    }

    private void MoveAnimationIsFinished()
    {
        isMoveAnimationFinished = true;
    }

    public FSM.StateDelegate OnMoveStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                {
                    targetPosition = node.GetPositionForNodeItem();
                    Vector3 position = transform.position;

                    if (Mathf.Approximately(targetPosition.x, position.x) && Mathf.Approximately(targetPosition.y, position.y) && Mathf.Approximately(targetPosition.z, position.z))
                    {
                        gameManager.Barrier.Remove(this);
                        return OnIdleStateUpdate;
                    }
                    MoveAddAnimation moveAddAnimation = new MoveAddAnimation(MoveOffsetTime, targetPosition - base.transform.position);
                    moveAddAnimation.AnimationFinishedDelegate = MoveAnimationIsFinished;
                    Animateur.PushAnimation(gameObject, moveAddAnimation);
                    break;
                }
            case FSM.Step.Update:
                if (isMoveAnimationFinished)
                {
                    isMoveAnimationFinished = false;
                    transform.position = targetPosition;
                    gameManager.Barrier.Remove(this);
                    return OnIdleStateUpdate;
                }
                break;
        }
        return null;
    }
}
