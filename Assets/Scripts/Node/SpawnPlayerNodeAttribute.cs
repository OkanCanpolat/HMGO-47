using UnityEngine;

public class SpawnPlayerNodeAttribute : SpawnNodeAttribute
{
    protected override void OnPawnSpawned(Pawn pawn)
    {
        base.OnPawnSpawned(pawn);

        PlayerPawn playerPawn = pawn as PlayerPawn;

        if (playerPawn != null)
        {
            playerPawn.SetCurrentNode(currentNode);
            playerPawn.SetTargetNode(currentNode);
            Transform playerPawnTransform = playerPawn.transform;
            playerPawnTransform.position = playerPawn.CurrentNode.transform.position;
            playerPawnTransform.rotation = playerPawn.CurrentOrientation.OrientationToQuaternion();
        }
    }
}
