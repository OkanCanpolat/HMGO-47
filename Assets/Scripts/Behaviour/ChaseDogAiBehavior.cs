using System;
using System.Collections.Generic;
using UnityEngine;

public class ChaseDogAiBehavior : AiBehavior
{
    private Pawn targetPawn;

    private Queue<Node> movementNodeQueue = new Queue<Node>();

    private int detectionDistance = 2;

    private AudioManager audioManager;
    public ChaseDogAiBehavior(AiPawn pawn)
        : base(pawn)
    {
        audioManager = UnityEngine.Object.FindObjectOfType<AudioManager>();
    }

    public virtual void OnPlayerPawnArrivalStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (step == FSM.Step.Enter)
        {
            movementNodeQueue.Enqueue(targetPawn.CurrentNode);
        }
    }

    public override void OnArrived(Node node)
    {
        if (movementNodeQueue.Count > 0 && movementNodeQueue.Peek() == node)
        {
            movementNodeQueue.Dequeue();
        }
        if (movementNodeQueue.Count == 0 && pawn.IsEmoting(PawnEmotionType.Exclamation))
        {
            pawn.UnsetEmotion(false);
            pawn.SetMovingMeshActive(false);
        }
    }

    public override void OnPlayerPawnUsedSecretPassage()
    {
        targetPawn = null;
        PlayerPawn playerPawn = gameManager.PlayerPawn;
        playerPawn.OnPawnArrivalStateUpdateNotifies = (PlayerPawn.OnPawnArrivalStateUpdate)Delegate.Remove(playerPawn.OnPawnArrivalStateUpdateNotifies, new PlayerPawn.OnPawnArrivalStateUpdate(OnPlayerPawnArrivalStateUpdate));
    }

    public override void OnPlayerPawnChangedDisguise()
    {
        targetPawn = null;
        PlayerPawn playerPawn = gameManager.PlayerPawn;
        playerPawn.OnPawnArrivalStateUpdateNotifies = (PlayerPawn.OnPawnArrivalStateUpdate)Delegate.Remove(playerPawn.OnPawnArrivalStateUpdateNotifies, new PlayerPawn.OnPawnArrivalStateUpdate(OnPlayerPawnArrivalStateUpdate));
    }

    public override Node EvaluateNextNode()
    {
        Node result = pawn.CurrentNode;
        PlayerPawn playerPawn = gameManager.PlayerPawn;
        if (pawn.IsPawnInLineOfSight(playerPawn) && pawn.IsPawnValidTarget(playerPawn))
        {
            result = playerPawn.CurrentNode;
        }
        else if (movementNodeQueue.Count > 0)
        {
            result = movementNodeQueue.Peek();
        }
        return result;
    }

    public override Orientation EvaluateNextOrientation()
    {
        Orientation result = pawn.CurrentOrientation;
        if (movementNodeQueue.Count > 0)
        {
            result = pawn.CurrentNode.GetOrientationToNode(movementNodeQueue.Peek());
        }
        return result;
    }

    public override void ExecuteMove()
    {
        PlayerPawn playerPawn = gameManager.PlayerPawn;
        if ((!pawn.IsPawnInLineOfSight(playerPawn) || !pawn.IsPawnValidTarget(playerPawn)) && targetPawn == null && movementNodeQueue.Count == 0 && pawn.IsPawnValidTarget(playerPawn) && !pawn.IsEmoting(PawnEmotionType.Exclamation))
        {
            List<Node> allNodesInOrientation = new List<Node>();
            pawn.CurrentNode.GetAllNodesInOrientation(pawn.CurrentOrientation, detectionDistance, ref allNodesInOrientation);
            if (allNodesInOrientation.Count == detectionDistance)
            {
                for (int i = 0; i < detectionDistance; i++)
                {
                    if (allNodesInOrientation[i].Pawns.Contains(playerPawn))
                    {
                        playerPawn.OnPawnArrivalStateUpdateNotifies = (PlayerPawn.OnPawnArrivalStateUpdate)Delegate.Combine(playerPawn.OnPawnArrivalStateUpdateNotifies, new PlayerPawn.OnPawnArrivalStateUpdate(OnPlayerPawnArrivalStateUpdate));
                        targetPawn = playerPawn;
                        for (int j = 0; j < detectionDistance; j++)
                        {
                            movementNodeQueue.Enqueue(allNodesInOrientation[j]);
                        }
                        audioManager.PlaySoundOnceAmong(pawn.SoundConfig.DogExclamationSounds, pawn.SoundConfig.DogExclamationVolume);
                        pawn.SetEmotion(PawnEmotionType.Exclamation);
                        pawn.SetMovingMeshActive(true);
                        return;
                    }
                }
            }
        }
        pawn.SetTargetNode(EvaluateNextNode());
    }
}
