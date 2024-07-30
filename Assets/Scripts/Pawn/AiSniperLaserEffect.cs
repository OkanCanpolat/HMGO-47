using System;
using UnityEngine;

public class AiSniperLaserEffect : MonoBehaviour
{
    public Transform LaserSightGeo;

    private float scaleOffset = 1f;

    private AiPawn aiPawn;
    private GameManager gameManager;

    public void Awake()
    {
        if (LaserSightGeo == null)
        {
            Debug.LogError("Laser sight attached to pawn but no geo was specified.");
        }
        gameManager = FindObjectOfType<GameManager>();

        scaleOffset = Mathf.Abs(LaserSightGeo.localPosition.z) / 4f;
        aiPawn = transform.GetComponent<AiPawn>();
        AiPawn pawn = aiPawn;
        pawn.OnPawnFirstFrameStateUpdateNotifies = (AiPawn.OnPawnFirstFrameStateUpdate)Delegate.Combine(pawn.OnPawnFirstFrameStateUpdateNotifies, new AiPawn.OnPawnFirstFrameStateUpdate(OnPawnFirstFrameStateUpdate));
        AiPawn aiPawn2 = aiPawn;
        aiPawn2.OnPawnMoveStateUpdateNotifies = (AiPawn.OnPawnMoveStateUpdate)Delegate.Combine(aiPawn2.OnPawnMoveStateUpdateNotifies, new AiPawn.OnPawnMoveStateUpdate(OnPawnMoveStateUpdate));
        AiPawn aiPawn3 = aiPawn;
        aiPawn3.OnPawnArrivalStateUpdateNotifies = (AiPawn.OnPawnArrivalStateUpdate)Delegate.Combine(aiPawn3.OnPawnArrivalStateUpdateNotifies, new AiPawn.OnPawnArrivalStateUpdate(OnPawnArrivalStateUpdate));
        AiPawn aiPawn4 = aiPawn;
        aiPawn4.OnPawnDeadStateUpdateNotifies = (AiPawn.OnPawnDeadStateUpdate)Delegate.Combine(aiPawn4.OnPawnDeadStateUpdateNotifies, new AiPawn.OnPawnDeadStateUpdate(OnPawnDeadStateUpdate));
        AiPawn aiPawn5 = aiPawn;
        aiPawn5.OnPawnKillStateUpdateNotifies = (AiPawn.OnPawnKillStateUpdate)Delegate.Combine(aiPawn5.OnPawnKillStateUpdateNotifies, new AiPawn.OnPawnKillStateUpdate(OnPawnKillStateUpdate));
        AiPawn aiPawn6 = aiPawn;
        aiPawn6.OnPawnHeardNoiseNotifies = (AiPawn.OnPawnHeardNoise)Delegate.Combine(aiPawn6.OnPawnHeardNoiseNotifies, new AiPawn.OnPawnHeardNoise(OnPawnHeardNoise));
    }

    public void Destroy()
    {
        AiPawn pawn = aiPawn;
        pawn.OnPawnFirstFrameStateUpdateNotifies = (AiPawn.OnPawnFirstFrameStateUpdate)Delegate.Remove(pawn.OnPawnFirstFrameStateUpdateNotifies, new AiPawn.OnPawnFirstFrameStateUpdate(OnPawnFirstFrameStateUpdate));
        AiPawn aiPawn2 = aiPawn;
        aiPawn2.OnPawnMoveStateUpdateNotifies = (AiPawn.OnPawnMoveStateUpdate)Delegate.Remove(aiPawn2.OnPawnMoveStateUpdateNotifies, new AiPawn.OnPawnMoveStateUpdate(OnPawnMoveStateUpdate));
        AiPawn aiPawn3 = aiPawn;
        aiPawn3.OnPawnArrivalStateUpdateNotifies = (AiPawn.OnPawnArrivalStateUpdate)Delegate.Remove(aiPawn3.OnPawnArrivalStateUpdateNotifies, new AiPawn.OnPawnArrivalStateUpdate(OnPawnArrivalStateUpdate));
        AiPawn aiPawn4 = aiPawn;
        aiPawn4.OnPawnDeadStateUpdateNotifies = (AiPawn.OnPawnDeadStateUpdate)Delegate.Remove(aiPawn4.OnPawnDeadStateUpdateNotifies, new AiPawn.OnPawnDeadStateUpdate(OnPawnDeadStateUpdate));
        AiPawn aiPawn5 = aiPawn;
        aiPawn5.OnPawnKillStateUpdateNotifies = (AiPawn.OnPawnKillStateUpdate)Delegate.Remove(aiPawn5.OnPawnKillStateUpdateNotifies, new AiPawn.OnPawnKillStateUpdate(OnPawnKillStateUpdate));
        AiPawn aiPawn6 = aiPawn;
        aiPawn6.OnPawnHeardNoiseNotifies = (AiPawn.OnPawnHeardNoise)Delegate.Remove(aiPawn6.OnPawnHeardNoiseNotifies, new AiPawn.OnPawnHeardNoise(OnPawnHeardNoise));
    }

    public void Update()
    {
        if (LaserSightGeo != null)
        {
            LaserSightGeo.rotation = Quaternion.LookRotation(LaserSightGeo.forward, Camera.main.transform.position - LaserSightGeo.position);
        }
    }

    public virtual void OnPawnFirstFrameStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (step == FSM.Step.Leave)
        {
            UpdateLaserSightEffect();
        }
    }

    public virtual void OnPawnMoveStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (step == FSM.Step.Enter)
        {
            UpdateLaserSightEffect();
        }
    }

    public virtual void OnPawnArrivalStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (step == FSM.Step.Enter)
        {
            UpdateLaserSightEffect();
        }
    }

    public virtual void OnPawnKillStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (step == FSM.Step.Leave)
        {
            UpdateLaserSightEffect();
        }
    }

    public virtual void OnPawnDeadStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (step == FSM.Step.Enter)
        {
            Destroy(LaserSightGeo.gameObject);
        }
    }

    public virtual void OnPawnHeardNoise(Node node, Barrier barrier)
    {
        UpdateLaserSightEffect();
        LaserSightGeo.gameObject.SetActive(false);
    }

    private void UpdateLaserSightEffect()
    {

        if (!(LaserSightGeo != null))
        {
            return;
        }
        if (aiPawn.IsBehaviorActive<GotoNodeAiBehavior>() && LaserSightGeo.gameObject.activeSelf)
        {
            LaserSightGeo.gameObject.SetActive(false);
        }
        else if (!aiPawn.IsBehaviorActive<GotoNodeAiBehavior>() && !LaserSightGeo.gameObject.activeSelf)
        {
            LaserSightGeo.gameObject.SetActive(true);
        }
        Node currentNode = aiPawn.CurrentNode;
        Node nodeInOrientation = aiPawn.CurrentNode.GetNodeInOrientation(aiPawn.CurrentOrientation, true);
        int num = 0;
        if (!aiPawn.IsBehaviorActive<GotoNodeAiBehavior>())
        {
            while (nodeInOrientation != null)
            {
                num++;
                currentNode = nodeInOrientation;
                bool flag = false;
                bool flag2 = nodeInOrientation.GetNodeAttribute<HidePawnNodeAttribute>() != null;

                for (int i = 0; i < nodeInOrientation.Pawns.Count; i++)
                {
                    if (!nodeInOrientation.Pawns[i].IsDead() && (!flag2 || !(nodeInOrientation.Pawns[i] == gameManager.PlayerPawn)))
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    for (int j = 0; j < nodeInOrientation.ArrivingPawns.Count; j++)
                    {
                        if (!nodeInOrientation.ArrivingPawns[j].IsDead() && (!flag2 || !(nodeInOrientation.ArrivingPawns[j] == gameManager.PlayerPawn)))
                        {
                            flag = true;
                        }
                    }
                }
                
                if (flag)
                {
                    break;
                }
                nodeInOrientation = currentNode.GetNodeInOrientation(aiPawn.CurrentOrientation, true);
            }
        }
        LaserSightGeo.localScale = new Vector3(LaserSightGeo.localScale.x, LaserSightGeo.localScale.y, num - scaleOffset);
    }
}
