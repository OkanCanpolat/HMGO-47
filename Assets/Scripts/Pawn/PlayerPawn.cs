using System.Collections.Generic;
using UnityEngine;
public enum NodeKillType
{
    NoKill, Single, Double, Multi, Mark
}
public class PlayerPawn : Pawn
{
    public delegate void OnPawnArrivalStateUpdate(FSM.Step a_Step, FSM.StateDelegate a_State);

    public MeshFilter MeshFilter;

    public List<Mesh> RegularMeshes = new List<Mesh>();

    public List<Mesh> PassiveMeshes = new List<Mesh>();

    public ParticleSystem BreakPassiveParticles;

    public OnPawnArrivalStateUpdate OnPawnArrivalStateUpdateNotifies;

    public PlayerPawnSoundConfig SoundConfig;

    private bool isKilledAnimationDone;

    public float SlowTimeValue = 0.45f;

    public float SelectedOffset = 0.2f;

    public Vector3 AiKillCameraShakeAmount = new Vector3(0.05f, 0f, 0.05f);

    public float AiKillCameraShakeDuration = 0.3f;

    public float EndMoveSoundDelay = 0.15f;

    private bool isMoveAnimationFinished;

    private bool isMoveAnimationStarted;

    public bool IsInTopCameraView;


    private Vector3 moveTargetPosition = Vector3.zero;

    private Vector3 moveStartPosition = Vector3.zero;

    private bool isOffsetMove;

    private GameManager gameManager;
    private NodeManager nodeManager;
    private InputManager inputManager;
    private AudioManager audioManager;

    protected override void Awake()
    {
        base.Awake();
        gameManager = FindObjectOfType<GameManager>();
        nodeManager = FindObjectOfType<NodeManager>();
        inputManager = FindObjectOfType<InputManager>();
        audioManager = FindObjectOfType<AudioManager>();
        gameManager.RegisterPlayerPawn(this);
        fsm.SetDebugName(gameObject.name);
        fsm.Start(OnFirstFrameStateUpdate);
        animator.SetBool("IsPlayer", true);
    }
    protected override void OnDestroy()
    {
        Time.timeScale = 1f;
        base.OnDestroy();
    }
    public override void OnSpawned()
    {
        SetMesh(PlayerMeshType.Normal);
    }
    public void SetMesh(PlayerMeshType meshType)
    {
        if (isPassive && (int)meshType < PassiveMeshes.Count)
        {
            MeshFilter.mesh = PassiveMeshes[(int)meshType];
        }
        else
        {
            MeshFilter.mesh = RegularMeshes[(int)meshType];
        }
    }
    public void OnKilled()
    {
        if (!fsm.IsStateActive(OnDeadStateUpdate))
        {
            gameManager.Barrier.Add(this);
            fsm.ForceTransition(OnDeadStateUpdate);
        }
    }
    public bool IsNodeValid(Node node)
    {
        if (!IsDead())
        {
            if (gameManager.ShouldSelectNodeForInteraction)
            {
                ThrowableTriggerNodeAttribute nodeAttribute = CurrentNode.GetNodeAttribute<ThrowableTriggerNodeAttribute>();
                if ((bool)nodeAttribute)
                {
                    float a_CrossHalfSize = 1.1f * nodeManager.Distance;
                    bool flag = false;
                    foreach (Pawn pawn in node.Pawns)
                    {
                        if (!pawn.IsDead())
                        {
                            flag = true;
                        }
                    }
                    if (node != CurrentNode && node.IsInCrossDistanceFrom(CurrentNode, a_CrossHalfSize) && !flag)
                    {
                        return true;
                    }
                    return false;
                }

                SniperTriggerNodeAttribute nodeAttribute2 = CurrentNode.GetNodeAttribute<SniperTriggerNodeAttribute>();

                if ((bool)nodeAttribute2)
                {
                    if (nodeAttribute2.TargetNodes.Contains(node))
                    {
                        return true;
                    }
                    return false;
                }
            }
            else
            {
                if (CurrentNode.IsConnectedTo(node) && !CurrentNode.IsNodeBlocked(node))
                {
                    return true;
                }
                SecretPassageNodeAttribute[] nodeAttributes = CurrentNode.GetNodeAttributes<SecretPassageNodeAttribute>();

                for (int i = 0; i < nodeAttributes.Length; i++)
                {
                    if (node == nodeAttributes[i].TargetNode)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private FSM.StateDelegate OnFirstFrameStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (step == FSM.Step.Update)
        {
            transform.position = CurrentNode.GetPositionForPawn(this);
            return OnIdleStateUpdate;
        }
        return null;
    }
    public FSM.StateDelegate OnIdleStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:

                if (!IsInTopCameraView)
                {
                    CameraSphericalMovement component = Camera.main.GetComponent<CameraSphericalMovement>();

                    if ((bool)TargetNode && (bool)component)
                    {
                        component.SetTarget(TargetNode.CameraPosition);
                    }
                }
                if ((bool)TargetNode && TargetNode.Music != null)
                {
                    audioManager.FadeIn(2, TargetNode.Music, false);
                }
                break;
            case FSM.Step.Transition:
                return state;
            case FSM.Step.Update:
            case FSM.Step.Leave:
                if (inputManager.IsPlayerAtStartPoint)
                {
                    MeshTransform.localPosition = new Vector3(0f, SelectedOffset, 0f);
                }
                else
                {
                    MeshTransform.localPosition = Vector3.zero;
                }
                break;
        }
        return null;
    }

    public FSM.StateDelegate OnInteractStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                {
                    Node lastClickedNode = gameManager.LastClickedNode;

                    CurrentNode.OnPlayerInteract(lastClickedNode, barrier);
                    SpawnAiNodeAttribute componentInChildren = lastClickedNode.GetComponentInChildren<SpawnAiNodeAttribute>();
                    if (componentInChildren != null && componentInChildren.SpawnMark)
                    {
                        Time.timeScale = SlowTimeValue;
                    }
                    break;
                }
            case FSM.Step.Update:
                if (barrier.IsFinished())
                {
                    gameManager.Barrier.Remove(this);
                    return OnIdleStateUpdate;
                }
                break;
            case FSM.Step.Leave:
                Time.timeScale = 1f;
                break;
        }
        return null;
    }
    public FSM.StateDelegate OnMoveStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                {
                    if (CurrentNode == TargetNode)
                    {
                        if (TargetNode.ArrivingPawns.Count == 0 && TargetNode.DepartingPawns.Count == 0)
                        {
                            gameManager.Barrier.Remove(this);
                            return OnIdleStateUpdate;
                        }
                        isOffsetMove = true;
                    }

                    else
                    {
                        isOffsetMove = false;
                    }
                    moveStartPosition = transform.position;
                    moveTargetPosition = TargetNode.GetPositionForPawn(this);
                    SpawnAiNodeAttribute componentInChildren = TargetNode.GetComponentInChildren<SpawnAiNodeAttribute>();
                    if (componentInChildren != null && componentInChildren.SpawnMark)
                    {
                        Time.timeScale = SlowTimeValue;
                    }
                    if (isOffsetMove)
                    {
                        MoveAddAnimation moveAddAnimation = new MoveAddAnimation(MoveTime, moveTargetPosition - transform.position);
                        moveAddAnimation.AnimationFinishedDelegate = MoveAnimationIsFinished;
                        Animateur.PushAnimation(gameObject, moveAddAnimation);
                        break;
                    }

                    SecretPassageNodeAttribute secretPassageNodeAttribute = null;
                    SecretPassageNodeAttribute[] nodeAttributes = CurrentNode.GetNodeAttributes<SecretPassageNodeAttribute>();
                    for (int i = 0; i < nodeAttributes.Length; i++)
                    {
                        if (nodeAttributes[i].TargetNode == TargetNode)
                        {
                            secretPassageNodeAttribute = nodeAttributes[i];
                            break;
                        }
                    }
                    if (secretPassageNodeAttribute != null && TargetNode == secretPassageNodeAttribute.TargetNode)
                    {
                        BaseAnimation baseAnimation = null;
                        if (secretPassageNodeAttribute.EntranceObjects.Count > 0)
                        {
                            baseAnimation = new MoveAddAnimation(MoveTime / 2, -5f * Vector3.up, Interpolation.EaseType.easeInBack);
                            baseAnimation.Delay = .5f * MoveTime / 2;
                            Animateur.PushAnimation(gameObject, baseAnimation);
                            audioManager.PlaySoundOnceAmong(secretPassageNodeAttribute.SoundConfig.PassageSounds, secretPassageNodeAttribute.SoundConfig.PassageVolume);
                            foreach (SecretPassageAnimation entranceObject in secretPassageNodeAttribute.EntranceObjects)
                            {
                                entranceObject.Open(.5f * MoveTime / 2, 0f);
                                entranceObject.Close(.5f * MoveTime / 2, 1.5f * MoveTime / 2);
                            }
                        }
                        baseAnimation = new MoveAddAnimation(0f, moveTargetPosition - transform.position);
                        if (secretPassageNodeAttribute.EntranceObjects.Count > 0)
                        {
                            baseAnimation.Delay = 1.5f * MoveTime / 2;
                        }
                        if (secretPassageNodeAttribute.ExitObjects.Count == 0)
                        {
                            baseAnimation.AnimationFinishedDelegate = MoveAnimationIsFinished;
                        }
                        Animateur.PushAnimation(gameObject, baseAnimation);
                        if (secretPassageNodeAttribute.ExitObjects.Count > 0)
                        {
                            foreach (SecretPassageAnimation exitObject in secretPassageNodeAttribute.ExitObjects)
                            {
                                exitObject.Open(.5f * MoveTime / 2, 2f * MoveTime / 2);
                                exitObject.Close(.5f * MoveTime / 2, 3.5f * MoveTime / 2);
                            }
                            baseAnimation = new MoveAddAnimation(MoveTime / 2, 5f * Vector3.up, Interpolation.EaseType.easeOutBack);
                            baseAnimation.Delay = 2.5f * MoveTime / 2;
                            baseAnimation.AnimationFinishedDelegate = MoveAnimationIsFinished;
                            Animateur.PushAnimation(base.gameObject, baseAnimation);
                        }

                        NodeKillType nodeKillType = PredictNodeKillType(TargetNode);

                        switch (nodeKillType)
                        {
                            case NodeKillType.NoKill:
                                audioManager.PlaySoundOnceAmong(SoundConfig.NoKillManholeMoveSounds, SoundConfig.NoKillManholeMoveVolume);
                                break;
                            case NodeKillType.Single:
                                audioManager.PlaySoundOnceAmong(SoundConfig.SingleKillManholeMoveSounds, SoundConfig.SingleKillManholeMoveVolume);
                                break;
                            case NodeKillType.Double:
                                audioManager.PlaySoundOnceAmong(SoundConfig.DoubleKillManholeMoveSounds, SoundConfig.DoubleKillManholeMoveVolume);
                                break;
                            case NodeKillType.Multi:
                                audioManager.PlaySoundOnceAmong(SoundConfig.TripleKillManholeMoveSounds, SoundConfig.TripleKillManholeMoveVolume);
                                break;
                            case NodeKillType.Mark:
                                audioManager.PlaySoundOnce(SoundConfig.MarkKilledSound, SoundConfig.MarkKilledVolume);
                                break;
                        }

                        foreach (AiPawn aiPawn in gameManager.AiPawns)
                        {
                            aiPawn.OnPlayerPawnUsedSecretPassage();
                        }
                    }
                    else
                    {
                        NodeKillType nodeKillType2 = PredictNodeKillType(TargetNode);
                        if (nodeKillType2 != 0)
                        {
                            animator.SetBool("KillMove", true);
                        }
                        else
                        {
                            animator.SetFloat("MoveAnimIndex", Random.Range(0, 5));
                            animator.SetBool("Move", true);
                        }
                        Vector3 eulerAngles = Quaternion.LookRotation(moveTargetPosition - moveStartPosition, Vector3.up).eulerAngles;
                        transform.eulerAngles = new Vector3(0f, eulerAngles.y, 0f);
                        switch (nodeKillType2)
                        {
                            case NodeKillType.NoKill:
                                audioManager.PlaySoundOnceAmong(SoundConfig.NoKillMoveSounds, SoundConfig.NoKillMoveVolume);
                                break;
                            case NodeKillType.Single:
                                audioManager.PlaySoundOnceAmong(SoundConfig.SingleKillMoveSounds, SoundConfig.SingleKillMoveVolume);
                                break;
                            case NodeKillType.Double:
                                audioManager.PlaySoundOnceAmong(SoundConfig.DoubleKillMoveSounds, SoundConfig.DoubleKillMoveVolume);
                                break;
                            case NodeKillType.Multi:
                                audioManager.PlaySoundOnceAmong(SoundConfig.TripleKillMoveSounds, SoundConfig.TripleKillMoveVolume);
                                break;
                            case NodeKillType.Mark:
                                audioManager.PlaySoundOnce(SoundConfig.MarkKilledSound, SoundConfig.MarkKilledVolume);
                                break;
                        }
                    }
                    break;
                }
            case FSM.Step.Update:
                if (!isOffsetMove && isMoveAnimationStarted)
                {
                    transform.position = Vector3.Lerp(moveStartPosition, moveTargetPosition, animator.GetFloat("MoveFactor"));
                }
                if (isMoveAnimationFinished)
                {
                    isMoveAnimationFinished = false;
                    transform.position = moveTargetPosition;
                    SetCurrentNode(TargetNode);
                    gameManager.Barrier.Remove(this);
                    Time.timeScale = 1f;
                    return OnIdleStateUpdate;
                }
                break;
            case FSM.Step.Leave:
                Time.timeScale = 1f;
                break;
        }
        return null;
    }
    public FSM.StateDelegate OnDeclareMoveStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (step == FSM.Step.Enter)
        {
            Node lastClickedNode = gameManager.LastClickedNode;
            SetTargetNode(lastClickedNode);
            if (TargetNode != CurrentNode)
            {
                CurrentNode.OnPawnDeparture(this, barrier);
            }

            gameManager.Barrier.Remove(this);
            return OnIdleStateUpdate;
        }
        return null;
    }
    public FSM.StateDelegate OnArrivalStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (OnPawnArrivalStateUpdateNotifies != null)
        {
            OnPawnArrivalStateUpdateNotifies(step, state);
        }
        switch (step)
        {
            case FSM.Step.Enter:
                CurrentNode.OnPawnArrival(this, barrier);
                return null;
            case FSM.Step.Update:
                if (barrier.IsFinished())
                {
                    gameManager.Barrier.Remove(this);
                    return OnIdleStateUpdate;
                }
                break;
        }
        return null;
    }
    public FSM.StateDelegate OnDeclareKillStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (step == FSM.Step.Enter)
        {
            foreach (AiPawn aiPawn in gameManager.AiPawns)
            {
                if (aiPawn != null && aiPawn.CurrentNode == CurrentNode && IsPawnValidTarget(aiPawn))
                {
                    aiPawn.OnKilled();
                }
            }
            gameManager.Barrier.Remove(this);
            return OnIdleStateUpdate;
        }
        return null;
    }
    public FSM.StateDelegate OnStartInteractStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                CurrentNode.OnPlayerStartInteract(barrier);
                return null;
            case FSM.Step.Update:
                if (barrier.IsFinished())
                {
                    gameManager.Barrier.Remove(this);
                    return OnIdleStateUpdate;
                }
                break;
        }
        return null;
    }
    public FSM.StateDelegate OnDeclarePushedStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                {
                    PushPawnsNodeAttribute nodeAttribute = CurrentNode.GetNodeAttribute<PushPawnsNodeAttribute>();
                    if (nodeAttribute == null)
                    {
                        Debug.LogError("Pawn declared to be pushed, but isn't on a node with a PushPawnsNodeAttribute.");
                    }
                    else
                    {
                        Node nodeInOrientation = CurrentNode.GetNodeInOrientation(nodeAttribute.Direction);
                        SetTargetNode(nodeInOrientation);
                        if (TargetNode != CurrentNode)
                        {
                            CurrentNode.OnPawnDeparture(this, barrier);
                        }
                    }
                    return null;
                }
            case FSM.Step.Update:
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                {
                    gameManager.Barrier.Remove(this);
                    return OnIdleStateUpdate;
                }
                break;
        }
        return null;
    }
    private NodeKillType PredictNodeKillType(Node node)
    {
        int num = 0;

        if (node.GetNodeAttribute<HidePawnNodeAttribute>() == null)
        {
            foreach (Pawn pawn in node.Pawns)
            {
                if (pawn.IsDead())
                {
                    continue;
                }

                DisguiseNodeAttribute nodeAttribute = node.GetNodeAttribute<DisguiseNodeAttribute>();

                if ((!(nodeAttribute != null) || !nodeAttribute.IsActive || nodeAttribute.Color != pawn.CurrentColor) && (CurrentColor != pawn.CurrentColor || (nodeAttribute != null && nodeAttribute.Color != pawn.CurrentColor)))
                {
                    if (pawn.gameObject.GetComponent<TargetPawnObjective>() != null)
                    {
                        return NodeKillType.Mark;
                    }
                    num++;
                }
            }
        }
        return (num <= 2) ? ((NodeKillType)num) : NodeKillType.Multi;
    }

    public void StartKilledAnimation(Pawn killer)
    {
        DeathDirectionNodeAttribute componentInChildren = CurrentNode.GetComponentInChildren<DeathDirectionNodeAttribute>();
        if (componentInChildren != null)
        {
            transform.rotation = componentInChildren.transform.rotation;
        }
        else
        {
            transform.rotation = killer.gameObject.transform.rotation;
        }

        FaceToCamera faceToCamera = MeshTransform.GetComponentInChildren<FaceToCamera>();
        faceToCamera.FaceCamera();
        faceToCamera.enabled = false;

        animator.SetFloat("KilledAnimIndex", Random.Range(0, 3));
        animator.SetBool("Killed", true);
    }
    private void MoveAnimationIsFinished()
    {
        isMoveAnimationFinished = true;
        OnKillAnimationDone();
    }

    private void OnKillAnimationDone()
    {
        foreach (AiPawn aiPawn in gameManager.AiPawns)
        {
            if (aiPawn != null && !gameManager.SkippingTurn && aiPawn.CurrentNode == TargetNode && IsPawnValidTarget(aiPawn))
            {
                aiPawn.StartKilledAnimation(this);
            }
        }
    }
    public override bool IsDead()
    {
        return fsm.IsStateActive(OnDeadStateUpdate);
    }
    public void SetDisguise(Material material, PawnColor color)
    {
        CurrentColor = color;
        FaceToCamera componentInChildren = MeshTransform.GetComponentInChildren<FaceToCamera>();
        MeshFilter componentInChildren2 = componentInChildren.transform.GetChild(0).GetComponentInChildren<MeshFilter>();
        componentInChildren2.GetComponent<Renderer>().material = material;

        foreach (AiPawn aiPawn in gameManager.AiPawns)
        {
            aiPawn.OnPlayerPawnChangedDisguise();
        }
    }
    public FSM.StateDelegate OnDeadStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:

                if (CurrentNode != null)
                {
                    CurrentNode.OnPlayerPawnKilled();
                }
                break;
            case FSM.Step.Update:
                if (isKilledAnimationDone)
                {
                    isKilledAnimationDone = false;
                }
                break;
        }
        return null;
    }
    protected override void OnKillMoveCameraShake()
    {
        int num = -1;

        foreach (AiPawn aiPawn in gameManager.AiPawns)
        {
            if (aiPawn != null && aiPawn.CurrentNode == base.TargetNode && IsPawnValidTarget(aiPawn))
            {
                num++;
            }
        }
        if (num > 0)
        {
            gameManager.ShakeCamera(AiKillCameraShakeAmount * num, AiKillCameraShakeDuration);
        }
    }

    protected override void OnAnimationStart(string animationName)
    {
        switch (animationName)
        {
            case "Move":
                isMoveAnimationStarted = true;
                break;
        }
    }

    protected override void OnAnimationEnd(string animationName)
    {
        switch (animationName)
        {
            case "Move":
                isMoveAnimationStarted = false;
                isMoveAnimationFinished = true;
                animator.SetBool("Move", false);
                animator.SetBool("KillMove", false);
                break;
            case "Killed":
                isKilledAnimationDone = true;
                break;
            case "Kill":
                OnKillAnimationDone();
                break;
        }
    }

}
