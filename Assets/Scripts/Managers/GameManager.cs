using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
    public delegate void OnPlayerPawnBreakPassive();
    public LevelDescription LevelDescription;
    public bool IsFsmDebugActivated;
    private FSM fsm = new FSM();
    private PlayerPawn playerPawn;
    private List<AiPawn> aiPawns = new List<AiPawn>();
    [HideInInspector] public List<GameObject> CollectedBriefcases = new List<GameObject>();
    [HideInInspector] public List<AiPawn> AiPawnSoundNotifications = new List<AiPawn>();
    [HideInInspector] public int AiPawnSoundNotificationsCount;
    private int aiPawnsSoundNotificationExpectedCount;
    private int turnsCount;
    [Inject] private NodeManager nodeManager;
    [Inject] private AudioManager audioManager;
    [Inject] private ConnectionLineDrawer lineDrawer;
    [Inject] private ViewManager viewManager;
    private Barrier barrier = new Barrier();
    private CameraEffects cameraEffects;
    private readonly int maxIterationsCount = 100;

    private bool isClickAllowed;

    private Node lastClickedNode;

    private Node nextClickedNode;

    public float replayDelay = 0.5f;

    public bool HasHidingSpotBeenUsed;

    [HideInInspector] public bool IsRoundIsShort;

    [HideInInspector] public bool ShouldSelectNodeForInteraction;

    [HideInInspector] public bool KilledUsingAConveyorBelt;

    public OnPlayerPawnBreakPassive OnPlayerPawnBreakPassiveNotifies;

    private bool isPassive;

    public float levelCompleteDelay = 0.8f;

    private float levelCompleteTimeOut;

    public bool SkippingTurn;

    public bool SkipNextTurn;

    private List<AiPawn> killedAiPawns = new List<AiPawn>();

    public AiPawnSoundConfig AiPawnSoundConfig;

    private List<Objective> objectives = new List<Objective>();

    private List<Objective> starObjectives = new List<Objective>();
    public PlayerPawn PlayerPawn => playerPawn;
    public List<AiPawn> AiPawns => aiPawns;
    public int TurnsCount => turnsCount;
    public bool IsClickAllowed => isClickAllowed;
    public Barrier Barrier => barrier;
    public Node LastClickedNode => lastClickedNode;
    public Node NextClickedNode => nextClickedNode;
    public bool InteractHasKilled { get; set; }
    public bool IsPassive
    {
        get
        {
            return isPassive;
        }
        set
        {
            isPassive = value;
        }
    }
    public List<Objective> Objectives => objectives;
    public List<Objective> StarObjectives => starObjectives;
    public List<AiPawn> KilledAiPawns => killedAiPawns;
    private void Awake()
    {
        nodeManager = FindObjectOfType<NodeManager>();
    }
    private void Start()
    {
        fsm.SetDebugName(gameObject.name);
        fsm.Start(OnFirstFrameStateUpdate);

        cameraEffects = Camera.main.GetComponent<CameraEffects>();
    }

    private void Update()
    {
		fsm.SetDebugActivation(IsFsmDebugActivated);

        int iterationCount = 0;
        do
        {
            fsm.Update();

            for (int i = AiPawns.Count - 1; i >= 0; i--)
            {
                AiPawn aiPawn = AiPawns[i];
                aiPawn.UpdateFsm();
            }

            if ((bool)playerPawn)
            {
                playerPawn.UpdateFsm();
            }
           
            foreach (Node node in nodeManager.Nodes)
            {
                NodeItem cachedNodeItem = node.CachedNodeItem;

                if (cachedNodeItem != null)
                {
                    cachedNodeItem.UpdateFsm();
                }
            }
            iterationCount++;
        }
        while (barrier.IsFinished() && iterationCount < maxIterationsCount);

        if (iterationCount >= maxIterationsCount)
        {
            Debug.LogError("GameManager : Endless update");
        }
    }

    public void RegisterPlayerPawn(PlayerPawn playerPawn)
    {
        this.playerPawn = playerPawn;
    }
    public void RegisterAiPawn(AiPawn aiPawn)
    {
        aiPawns.Add(aiPawn);
    }
    public List<AiPawn> GetAiPawnsOnNode(Node node)
    {
        List<AiPawn> list = new List<AiPawn>();

        foreach (AiPawn aiPawn in AiPawns)
        {
            if (aiPawn.CurrentNode == node)
            {
                list.Add(aiPawn);
            }
        }
        return list;
    }
    public void OnPlayerBreakPassive()
    {
        for (int i = 0; i < AiPawns.Count; i++)
        {
            AiPawns[i].OnPlayerBreakPassive();
        }
        PlayerPawn.OnPlayerBreakPassive();

        if (OnPlayerPawnBreakPassiveNotifies != null)
        {
            OnPlayerPawnBreakPassiveNotifies();
        }
    }
    public void OnNodeClicked(Node node)
    {
        if (isClickAllowed)
        {
            if (playerPawn.IsNodeValid(node))
            {
                lastClickedNode = node;
            }
        }
        else if (!ShouldSelectNodeForInteraction)
        {
            nextClickedNode = node;
        }
    }

    public void OnAiPawnKilled(AiPawn a_AiPawn)
    {
        aiPawns.Remove(a_AiPawn);
        killedAiPawns.Add(a_AiPawn);

        for (int i = 0; i < AiPawns.Count; i++)
        {
            AiPawns[i].OnAiPawnKilled(a_AiPawn);
        }
        PlayerPawn.OnAiPawnKilled(a_AiPawn);
    }
    private FSM.StateDelegate OnFirstFrameStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (step == FSM.Step.Update)
        {
            foreach (Node node in nodeManager.Nodes)
            {
                node.ComputeOptimalPawnsTargetPositions();
            }
           
            return OnWaitingCameraStateUpdate;
        }
        if(step == FSM.Step.Leave)
        {
            starObjectives.Sort((Objective x, Objective y) => (x.IsMainObjective ^ y.IsMainObjective) ? ((!x.IsMainObjective) ? 1 : (-1)) : x.ObjectiveIndex.CompareTo(y.ObjectiveIndex));
        }
        return null;
    }
    private FSM.StateDelegate OnWaitingCameraStateUpdate(FSM.Step a_Step, FSM.StateDelegate a_State)
    {
        switch (a_Step)
        {
            case FSM.Step.Enter:
                barrier.Add(this);
                return null;

            case FSM.Step.Update:
                if (!lineDrawer.InConnectionAnim)
                {
                    return OnNodeSelectionStateUpdate;
                }

                return null;

            case FSM.Step.Leave:
                barrier.Remove(this);
                return null;

            default:
                return null;
        }
    }

    private FSM.StateDelegate OnNodeSelectionStateUpdate(FSM.Step a_Step, FSM.StateDelegate a_State)
    {
        switch (a_Step)
        {
            case FSM.Step.Enter:
                lastClickedNode = null;
                isClickAllowed = true;
                IsRoundIsShort = ShouldSelectNodeForInteraction;
                playerPawn.CurrentNode.OnEnterNodeSelection(ShouldSelectNodeForInteraction);
                barrier.Add(this);
                return null;

            case FSM.Step.Update:
                if (isClickAllowed)
                {
                    if (lastClickedNode == null && nextClickedNode != null)
                    {
                        if (playerPawn.IsNodeValid(nextClickedNode))
                        {
                            lastClickedNode = nextClickedNode;
                        }
                        nextClickedNode = null;
                    }
                }
                if (lastClickedNode != null)
                {
                    isClickAllowed = false;
                    EnvironmentalKillNodeAttribute nodeAttribute = lastClickedNode.GetNodeAttribute<EnvironmentalKillNodeAttribute>();
                    bool flag = false;
                    if (nodeAttribute != null)
                    {
                        for (int i = 0; i < nodeAttribute.TargetNodes.Count; i++)
                        {
                            SpawnAiNodeAttribute nodeAttribute2 = nodeAttribute.TargetNodes[i].GetNodeAttribute<SpawnAiNodeAttribute>();
                            
                            if (nodeAttribute2 != null && nodeAttribute2.SpawnMark)
                            {
                                flag = true;
                            }
                        }
                    }
                    bool flag2 = false;
                    for (int j = 0; j < lastClickedNode.Pawns.Count; j++)
                    {
                        if (lastClickedNode.Pawns[j].GetComponent<TargetPawnObjective>() != null)
                        {
                            flag2 = true;
                        }
                    }
                    if ((lastClickedNode.GetNodeAttribute<ExtractionPointNodeAttribute>() != null && !ShouldSelectNodeForInteraction) || flag || flag2)
                    {
                        //GameSingleton<ViewManager>.Instance.NotifyMovingToEndNode();
                    }
                    if (ShouldSelectNodeForInteraction)
                    {
                        ShouldSelectNodeForInteraction = false;
                        return OnPlayerInteractStateUpdate;
                    }
                    return OnPlayerDeclareMoveStateUpdate;
                }
                return null;
            case FSM.Step.Leave:
                foreach (Node node in nodeManager.Nodes)
                {
                    node.OnLeaveNodeSelection();
                }
                barrier.Remove(this);
                return null;
            default:
                return null;
        }
    }
    private FSM.StateDelegate OnPlayerDeclareMoveStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                turnsCount++;
                barrier.Add(PlayerPawn);
                playerPawn.RequestState(playerPawn.OnDeclareMoveStateUpdate);
                return null;
            case FSM.Step.Update:
                if (barrier.IsFinished())
                {
                    foreach (Node node in nodeManager.Nodes)
                    {
                        node.ComputeOptimalPawnsTargetPositions();
                    }
                    return OnPlayerMoveStateUpdate;
                }
                return null;
            default:
                return null;
        }
    }
    private FSM.StateDelegate OnPlayerMoveStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                barrier.Add(PlayerPawn);
                playerPawn.RequestState(playerPawn.OnMoveStateUpdate);
                foreach (AiPawn aiPawn in AiPawns)
                {
                    barrier.Add(aiPawn);
                    aiPawn.RequestState(aiPawn.OnMoveStateUpdate);
                }
                foreach (Node node in nodeManager.Nodes)
                {
                    NodeItem cachedNodeItem = node.CachedNodeItem;
                    if ((bool)cachedNodeItem)
                    {
                        barrier.Add(cachedNodeItem);
                        cachedNodeItem.RequestState(cachedNodeItem.OnMoveStateUpdate);
                    }
                }
                return null;
            case FSM.Step.Update:
                if (barrier.IsFinished())
                {
                    return OnPlayerArrivalStateUpdate;
                }
                return null;
            default:
                return null;
        }
    }
    private FSM.StateDelegate OnPlayerArrivalStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                barrier.Add(PlayerPawn);
                playerPawn.RequestState(playerPawn.OnArrivalStateUpdate);
                return null;
            case FSM.Step.Update:
                if (barrier.IsFinished())
                {
                    return OnPlayerDeclareKillStateUpdate;
                }
                return null;
            default:
                return null;
        }
    }
    private FSM.StateDelegate OnPlayerDeclareKillStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                barrier.Add(PlayerPawn);
                playerPawn.RequestState(playerPawn.OnDeclareKillStateUpdate);
                return null;
            case FSM.Step.Update:
                if (barrier.IsFinished())
                {
                    bool flag = false;
                    foreach (AiPawn aiPawn in AiPawns)
                    {
                        if (aiPawn.IsBeingKilled())
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        foreach (Node node in nodeManager.Nodes)
                        {
                            node.ComputeOptimalPawnsTargetPositions();
                        }
                        return OnPlayerKillStateUpdate;
                    }
                    return OnPlayerStartInteractStateUpdate;
                }
                return null;
            default:
                return null;
        }
    }
    private FSM.StateDelegate OnPlayerKillStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                if (PlayerPawn.WasOnPushingNode())
                {
                    KilledUsingAConveyorBelt = true;
                }
                barrier.Add(PlayerPawn);
                playerPawn.RequestState(playerPawn.OnMoveStateUpdate);
                foreach (AiPawn aiPawn in AiPawns)
                {
                    if (aiPawn.IsBeingKilled())
                    {
                        barrier.Add(aiPawn);
                        aiPawn.RequestState(aiPawn.OnDeadStateUpdate);
                    }
                    else
                    {
                        barrier.Add(aiPawn);
                        aiPawn.RequestState(aiPawn.OnMoveStateUpdate);
                    }
                }
                return null;
            case FSM.Step.Update:
                if (barrier.IsFinished())
                {
                    return OnPlayerStartInteractStateUpdate;
                }
                return null;
            default:
                return null;
        }
    }
    private FSM.StateDelegate OnPlayerStartInteractStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                barrier.Add(PlayerPawn);
                playerPawn.RequestState(playerPawn.OnStartInteractStateUpdate);
                return null;
            case FSM.Step.Update:
                if (barrier.IsFinished())
                {
                    if (InteractHasKilled)
                    {
                        InteractHasKilled = false;
                        return OnPlayerInteractKillStateUpdate;
                    }
                    if (playerPawn.IsOnPushingNode())
                    {
                        return OnPlayerDeclarePushedStateUpdate;
                    }
                    return OnPlayerFinishStateUpdate;
                }
                return null;
            default:
                return null;
        }
    }
    private FSM.StateDelegate OnPlayerDeclarePushedStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                {
                    for (int i = 0; i < AiPawns.Count; i++)
                    {
                        if (AiPawns[i].AnticipateKillNextMove())
                        {
                            return OnPlayerFinishStateUpdate;
                        }
                    }
                    barrier.Add(playerPawn);
                    playerPawn.RequestState(playerPawn.OnDeclarePushedStateUpdate);
                    return null;
                }
            case FSM.Step.Update:
                if (barrier.IsFinished())
                {
                    foreach (Node node in nodeManager.Nodes)
                    {
                        node.ComputeOptimalPawnsTargetPositions();
                    }
                    return OnPlayerMoveStateUpdate;
                }
                return null;
            default:
                return null;
        }
    }

    private FSM.StateDelegate OnPlayerInteractStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                barrier.Add(PlayerPawn);
                playerPawn.RequestState(playerPawn.OnInteractStateUpdate);
                return null;

            case FSM.Step.Update:
                if (barrier.IsFinished())
                {
                    if (InteractHasKilled)
                    {
                        InteractHasKilled = false;
                        return OnPlayerInteractKillStateUpdate;
                    }
                    return OnAiRotateStateUpdate;
                }
                return null;
            default:
                return null;
        }
    }
    private FSM.StateDelegate OnPlayerInteractKillStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:

                barrier.Add(PlayerPawn);
                playerPawn.RequestState(playerPawn.OnMoveStateUpdate);

                foreach (AiPawn aiPawn in AiPawns)
                {
                    if (aiPawn.IsBeingKilled())
                    {
                        barrier.Add(aiPawn);
                        aiPawn.RequestState(aiPawn.OnDeadStateUpdate);
                    }
                    else
                    {
                        barrier.Add(aiPawn);
                        aiPawn.RequestState(aiPawn.OnMoveStateUpdate);
                    }
                }
                return null;
            case FSM.Step.Update:

                if (barrier.IsFinished())
                {
                    return OnPlayerFinishStateUpdate;
                }
                return null;
            default:
                return null;
        }
    }
    private FSM.StateDelegate OnPlayerFinishStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:

                if (starObjectives[0].IsComplete())
                {
                    barrier.Add(this);
                    levelCompleteTimeOut = Time.time + levelCompleteDelay;
                    viewManager.NotifyLevelObjectivesCompleted();
                    return null;
                }

                if (IsRoundIsShort)
                {
                    return OnAiRotateStateUpdate;
                }

                return OnAiDeclareMoveStateUpdate;
            case FSM.Step.Update:
                if (levelCompleteTimeOut != 0f && Time.time > levelCompleteTimeOut)
                {
                    //GameSingleton<GameStructure>.Instance.NotifyCompleteLevel();
                    viewManager.NotifyLevelComplete();
                    //GameSingleton<AchievementManager>.Instance.ValidateChapterCompletion();
                    //GameSingleton<AchievementManager>.Instance.ValidateGameCompletion();
                    //GameSingleton<AchievementManager>.Instance.UnlockHintRebel();
                    levelCompleteTimeOut = 0f;
                }
                return null;
            default:
                return null;
        }
    }

    private FSM.StateDelegate OnAiRotateStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                AiPawnSoundNotifications.Clear();
                AiPawnSoundNotificationsCount = 0;
                aiPawnsSoundNotificationExpectedCount = 0;

                foreach (AiPawn aiPawn in AiPawns)
                {
                    if (IsRoundIsShort)
                    {
                        if (aiPawn != null && (aiPawn.IsEmoting(PawnEmotionType.Question) || aiPawn.JustWitnessedEnvironmentalKill))
                        {
                            barrier.Add(aiPawn);
                            aiPawnsSoundNotificationExpectedCount++;
                            aiPawn.RequestState(aiPawn.OnRotateStateUpdate);
                            aiPawn.JustWitnessedEnvironmentalKill = false;
                        }
                    }
                    else
                    {
                        barrier.Add(aiPawn);
                        aiPawnsSoundNotificationExpectedCount++;
                        aiPawn.RequestState(aiPawn.OnRotateStateUpdate);
                    }
                }
                return null;
            case FSM.Step.Update:
                if (AiPawnSoundNotifications.Count == aiPawnsSoundNotificationExpectedCount)
                {
                    if (AiPawnSoundNotificationsCount == 1)
                    {
                        audioManager.PlaySoundOnceAmong(AiPawnSoundConfig.SingleRotateSounds, AiPawnSoundConfig.SingleRotateVolume);
                    }
                    else if (AiPawnSoundNotificationsCount == 2)
                    {
                        audioManager.PlaySoundOnceAmong(AiPawnSoundConfig.DoubleRotateSounds, AiPawnSoundConfig.DoubleRotateVolume);
                    }
                    else if (AiPawnSoundNotificationsCount >= 3)
                    {
                        audioManager.PlaySoundOnceAmong(AiPawnSoundConfig.TripleRotateSounds, AiPawnSoundConfig.TripleRotateVolume);
                    }
                    AiPawnSoundNotifications.Clear();
                    AiPawnSoundNotificationsCount = 0;
                }
                if (barrier.IsFinished())
                {
                    return OnAiFinishStateUpdate;
                }
                return null;
            default:
                return null;
        }
    }
    private FSM.StateDelegate OnAiFinishStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                return null;
            case FSM.Step.Update:
                if (SkipNextTurn)
                {
                    SkipNextTurn = false;
                    SkippingTurn = true;
                    return OnAiDeclareMoveStateUpdate;
                }
                return OnTurnFinishStateUpdate;
            default:
                return null;
        }
    }
    private FSM.StateDelegate OnTurnFinishStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                SkippingTurn = false;
                return null;
            case FSM.Step.Update:
                if (!playerPawn.IsDead())
                {
                    foreach (Node node in nodeManager.Nodes)
                    {
                        NodeAttribute[] cachedNodeAttributes = node.CachedNodeAttributes;

                        foreach (NodeAttribute nodeAttribute in cachedNodeAttributes)
                        {
                            nodeAttribute.OnBeginningOfPlayerTurn();
                        }
                    }
                    return OnNodeSelectionStateUpdate;
                }
                return null;
            default:
                return null;
        }
    }

    private FSM.StateDelegate OnAiDeclareMoveStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                foreach (AiPawn aiPawn in AiPawns)
                {
                    barrier.Add(aiPawn);
                    aiPawn.RequestState(aiPawn.OnDeclareMoveStateUpdate);
                }
                return null;
            case FSM.Step.Update:
                if (barrier.IsFinished())
                {
                    foreach (Node node in nodeManager.Nodes)
                    {
                        node.ComputeOptimalPawnsTargetPositions();
                    }
                    return OnAiMoveStateUpdate;
                }
                return null;
            default:
                return null;
        }
    }

    private FSM.StateDelegate OnAiMoveStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                AiPawnSoundNotifications.Clear();
                AiPawnSoundNotificationsCount = 0;
                aiPawnsSoundNotificationExpectedCount = 0;

                foreach (AiPawn aiPawn in AiPawns)
                {
                    barrier.Add(aiPawn);
                    aiPawnsSoundNotificationExpectedCount++;
                    aiPawn.RequestState(aiPawn.OnMoveStateUpdate);
                }
                barrier.Add(playerPawn);
                playerPawn.RequestState(playerPawn.OnMoveStateUpdate);
                foreach (Node node in nodeManager.Nodes)
                {
                    NodeItem cachedNodeItem = node.CachedNodeItem;
                    if ((bool)cachedNodeItem)
                    {
                        barrier.Add(cachedNodeItem);
                        cachedNodeItem.RequestState(cachedNodeItem.OnMoveStateUpdate);
                    }
                }
                return null;
            case FSM.Step.Update:
                if (AiPawnSoundNotifications.Count == aiPawnsSoundNotificationExpectedCount)
                {
                    if (AiPawnSoundNotificationsCount == 1)
                    {
                        audioManager.PlaySoundOnceAmong(AiPawnSoundConfig.SingleMoveSounds, AiPawnSoundConfig.SingleMoveVolume);
                    }
                    else if (AiPawnSoundNotificationsCount == 2)
                    {
                        audioManager.PlaySoundOnceAmong(AiPawnSoundConfig.DoubleMoveSounds, AiPawnSoundConfig.DoubleMoveVolume);
                    }
                    else if (AiPawnSoundNotificationsCount >= 3)
                    {
                        audioManager.PlaySoundOnceAmong(AiPawnSoundConfig.TripleMoveSounds, AiPawnSoundConfig.TripleMoveVolume);
                    }
                    AiPawnSoundNotifications.Clear();
                    AiPawnSoundNotificationsCount = 0;
                }
                if (barrier.IsFinished())
                {
                    return OnAiKillStateUpdate;
                }
                return null;
            default:
                return null;
        }
    }
    private FSM.StateDelegate OnAiKillStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                foreach (AiPawn aiPawn in AiPawns)
                {
                    barrier.Add(aiPawn);
                    aiPawn.RequestState(aiPawn.OnKillStateUpdate);
                }
                return null;
            case FSM.Step.Update:
                if (barrier.IsFinished())
                {
                    return OnAiArrivalStateUpdate;
                }
                return null;
            default:
                return null;
        }
    }
    private FSM.StateDelegate OnAiArrivalStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                foreach (AiPawn aiPawn in AiPawns)
                {
                    barrier.Add(aiPawn);
                    aiPawn.RequestState(aiPawn.OnArrivalStateUpdate);
                }
                return null;
            case FSM.Step.Update:
                if (barrier.IsFinished())
                {
                    return OnAiDeclarePushedStateUpdate;
                }
                return null;
            default:
                return null;
        }
    }
    private FSM.StateDelegate OnAiDeclarePushedStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                {
                    bool flag = false;
                    for (int i = 0; i < AiPawns.Count; i++)
                    {
                        if (AiPawns[i].IsOnPushingNode())
                        {
                            flag = true;
                            barrier.Add(AiPawns[i]);
                            AiPawns[i].RequestState(AiPawns[i].OnDeclarePushedStateUpdate);
                        }
                    }
                    if (!flag)
                    {
                        return OnAiRotateStateUpdate;
                    }
                    return null;
                }
            case FSM.Step.Update:
                if (barrier.IsFinished())
                {
                    foreach (Node node in nodeManager.Nodes)
                    {
                        node.ComputeOptimalPawnsTargetPositions();
                    }
                    return OnAiMoveStateUpdate;
                }
                return null;
            default:
                return null;
        }
    }

    public void RegisterObjective(Objective a_Objective)
    {
        objectives.Add(a_Objective);
    }

    public void RegisterStarObjective(Objective a_Objective)
    {
        starObjectives.Add(a_Objective);
    }

    public void ShakeCamera(Vector3 shakeAmount, float duration)
    {
        if (cameraEffects != null && duration > 0f)
        {
            cameraEffects.Shake(shakeAmount, duration);
        }
    }
}
