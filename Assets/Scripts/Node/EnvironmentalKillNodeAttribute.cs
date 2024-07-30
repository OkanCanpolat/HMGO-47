using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnvironmentalKillNodeAttribute : NodeAttribute
{
    public EnvironmentalKillObject EnvironmentObject;

    public List<Node> TargetNodes = new List<Node>();

    public bool DestroysTargetNodesConnections;

    private Barrier barrier;
    [Inject] private NodeManager nodeManager;

    private void Awake()
    {
        if (EnvironmentObject != null)
        {
            EnvironmentObject.TiedNodeAttribute = this;
        }
    }

    public override void OnPlayerSniperShot(Barrier barrier)
    {
        base.OnPlayerSniperShot(barrier);
        this.barrier = barrier;
        this.barrier.Add(this);
        EnvironmentObject.Trigger();
    }

    public void OnEnvironmentalKill()
    {
        int num = 0;

        for (int i = 0; i < TargetNodes.Count; i++)
        {
            for (int num2 = TargetNodes[i].Pawns.Count - 1; num2 >= 0; num2--)
            {
                AiPawn aiPawn = TargetNodes[i].Pawns[num2] as AiPawn;

                if (aiPawn != null)
                {
                    num++;

                    aiPawn.StartKilledAnimation(aiPawn);
                    aiPawn.OnKilled();

                    gameManager.InteractHasKilled = true;
                }
            }
            NotifyAdjacentAis();

            if (DestroysTargetNodesConnections)
            {
                nodeManager.RemoveNode(TargetNodes[i]);
                nodeManager.RemoveNode(currentNode);
            }
        }
    }

    public void OnAnimationEnd()
    {
        barrier.Remove(this);
    }

    private void NotifyAdjacentAis()
    {
        List<AiPawn> list = new List<AiPawn>();
        
        float a_CubeHalfSize = 1.1f * nodeManager.Distance;
        for (int i = 0; i < TargetNodes.Count; i++)
        {
            foreach (Node node in nodeManager.Nodes)
            {
                Vector3 position = TargetNodes[i].transform.position;
                position.y = 0f;
                Vector3 position2 = node.transform.position;
                position2.y = 0f;
                if (!node || !node.IsInCubeBoxDistanceFrom(TargetNodes[i], a_CubeHalfSize))
                {
                    continue;
                }
                List<AiPawn> aiPawnsOnNode = gameManager.GetAiPawnsOnNode(node);

                foreach (AiPawn item in aiPawnsOnNode)
                {
                    if (!list.Contains(item))
                    {
                        item.OnEnvironmentalKill(TargetNodes[i], barrier);
                        list.Add(item);
                    }
                }
            }
        }
    }
}
