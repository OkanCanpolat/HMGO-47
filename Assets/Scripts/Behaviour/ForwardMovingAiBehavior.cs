using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardMovingAiBehavior : AiBehavior
{
    public ForwardMovingAiBehavior(AiPawn pawn)
        : base(pawn)
    {
    }

    public override Node EvaluateNextNode()
    {
        PlayerPawn playerPawn = gameManager.PlayerPawn;

        if (pawn.IsPawnInLineOfSight(playerPawn) && pawn.IsPawnValidTarget(playerPawn))
        {
            return playerPawn.CurrentNode;
        }

        Orientation currentOrientation = pawn.CurrentOrientation;
        Node currentNode = pawn.CurrentNode;
        Node nodeInOrientation = currentNode.GetNodeInOrientation(currentOrientation);

        if (nodeInOrientation != null)
        {
            return nodeInOrientation;
        }
        return pawn.CurrentNode;
    }

    public override Orientation EvaluateNextOrientation()
    {
        Orientation orientation = pawn.CurrentOrientation;
        Node currentNode = pawn.CurrentNode;
        Node nodeInOrientation = currentNode.GetNodeInOrientation(orientation);

        if (nodeInOrientation == null)
        {
            orientation = orientation.OppositeOrientation();
        }
        return orientation;
    }
}
