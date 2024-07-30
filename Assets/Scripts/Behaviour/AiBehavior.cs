using UnityEngine;
public class AiBehavior 
{
    protected AiPawn pawn;

    protected bool isActivated;

    protected GameManager gameManager;
    public AiBehavior(AiPawn pawn)
    {
        this.pawn = pawn;
        gameManager = Object.FindObjectOfType<GameManager>();
    }

    public virtual void Activate()
    {
        isActivated = true;
    }

    public virtual void Deactivate()
    {
        isActivated = false;
    }

    public virtual Pawn EvaluateKillTarget(Node targetNode = null)
    {
        PlayerPawn playerPawn = gameManager.PlayerPawn;

        Node node = ((!(targetNode == null)) ? targetNode : pawn.CurrentNode);

        if (playerPawn.CurrentNode == node)
        {
            return playerPawn;
        }
        return null;
    }

    public virtual Node EvaluateNextNode()
    {
        return pawn.CurrentNode;
    }

    public virtual Orientation EvaluateNextOrientation()
    {
        return pawn.CurrentOrientation;
    }

    public virtual void ExecuteMove()
    {
        pawn.SetTargetNode(EvaluateNextNode());
    }

    public virtual void ExecuteRotation()
    {
        pawn.SetTargetOrientation(EvaluateNextOrientation());
    }

    public virtual void OnArrived(Node node)
    {
    }

    public virtual void OnPlayerPawnChangedDisguise()
    {
    }

    public virtual void OnPlayerPawnUsedSecretPassage()
    {
    }
}
