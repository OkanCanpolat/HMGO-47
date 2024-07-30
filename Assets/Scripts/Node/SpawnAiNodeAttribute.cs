using UnityEngine;

public class SpawnAiNodeAttribute : SpawnNodeAttribute
{
    public Orientation Orientation = Orientation.PositiveZ;

    public PatrolPath PatrolPath;

    [Range(-1f, 3f)]
    public int ForcedDeathAnimation = -1;

    public bool SpawnMark;

    protected override void OnPawnSpawned(Pawn pawn)
    {
        base.OnPawnSpawned(pawn);
        AiPawn aiPawn = pawn as AiPawn;
        if (!aiPawn)
        {
            return;
        }
        aiPawn.SetCurrentNode(currentNode);
        aiPawn.SetTargetNode(currentNode);
        aiPawn.SetCurrentOrientation(Orientation);
        Transform transform = aiPawn.transform;
        transform.position = aiPawn.CurrentNode.transform.position;
        transform.rotation = aiPawn.CurrentOrientation.OrientationToQuaternion();
        aiPawn.PatrolPath = PatrolPath;
        aiPawn.ForcedDeathAnimationIndex = ForcedDeathAnimation;
        AiBehavior initialBehavior = aiPawn.GetInitialBehavior();
        aiPawn.SetBehavior(initialBehavior);

        if (!(initialBehavior is PatrolAiBehavior) && PatrolPath != null)
        {
            Debug.LogError("ERROR: PatrolPath attached on AiPawn Spawner that is not of PatrolAiBehaviour type.");
        }
        if (SpawnMark)
        {
            GameObject gameObject = GameObject.Find("Objectives");
            if ((bool)gameObject)
            {
                KillTheMarkObjective killTheMarkObjective = gameObject.AddComponent<KillTheMarkObjective>();
                gameManager.RegisterStarObjective(killTheMarkObjective);
                killTheMarkObjective.SetTarget(aiPawn);
            }
        }
        if (aiPawn.InitialBehavior == AiPawn.MovementBehavior.CounterClockwiseRotating)
        {
            Transform child = aiPawn.MeshTransform.GetChild(0);
            Vector3 eulerAngles = child.localRotation.eulerAngles;
            eulerAngles.y = 0f - eulerAngles.y;
            child.localRotation = Quaternion.Euler(eulerAngles);
            Vector3 localScale = child.localScale;
            localScale.x = 0f - localScale.x;
            child.localScale = localScale;
        }
    }
}
