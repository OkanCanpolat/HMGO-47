using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class AiPawn : Pawn
{
    public enum MovementBehavior
    {
        Stationary,
        ForwardMoving,
        ClockwiseRotating,
        CounterClockwiseRotating,
        _Unused,
        HalfTurnRotating,
        Partners,
        Sniper,
        ChaseDog,
        Patrol,
        StrafingPartners
    }

    public delegate void OnPawnFirstFrameStateUpdate(FSM.Step step, FSM.StateDelegate state);

    public delegate void OnPawnMoveStateUpdate(FSM.Step step, FSM.StateDelegate state);

    public delegate void OnPawnRotateStateUpdate(FSM.Step step, FSM.StateDelegate state);

    public delegate void OnPawnArrivalStateUpdate(FSM.Step step, FSM.StateDelegate state);

    public delegate void OnPawnKillStateUpdate(FSM.Step step, FSM.StateDelegate state);

    public delegate void OnPawnDeadStateUpdate(FSM.Step step, FSM.StateDelegate state);

    public delegate void OnPawnHeardNoise(Node node, Barrier barrier);

    public GameObject DontKillIconPrefab;

    public float DontKillIconHeight = 3f;

    public float DontKillIconMaxHeight = 4.2f;

    protected GameObject dontKillIcon;

    public AiPawnSoundConfig SoundConfig;

    public bool HasShield;

    [HideInInspector]
    public int ForcedDeathAnimationIndex = -1;

    private PatrolPath patrolPath;

    public List<PawnEmotion> EmotionPrefabs = new List<PawnEmotion>();

    private PawnEmotion currentPawnEmotion;

    public float EmotionTweenTime = 0.8f;

    private GameObject emotionIconObject;

    public float EmotionIconHeightStart = 4f;

    public float EmotionIconHeightEnd = 3f;

    public float ExclamatingJumpHeight = 0.5f;

    public float ExclamatingJumpTime = 0.1f;

    public float ExclamatingWaitTime = 0.6f;

    private Vector3 initialPosition;

    public float QuestionningTurnAngle = 10f;

    public float QuestionningTurnTime = 0.2f;

    public float QuestionningWaitTime = 0.6f;

    private Quaternion initialRotation;

    private bool isOddRotation;

    private bool isLeavingIdle;

    private bool justWitnessedEnvironmentalKill;

    public GameObject[] MeshVariations;

    public GameObject[] MovingMeshVariations;

    private GameObject chosenMeshVariation;

    private GameObject chosenMovingMeshVariation;

    public MovementBehavior InitialBehavior;

    protected AiBehavior currentBehavior;

    public OnPawnFirstFrameStateUpdate OnPawnFirstFrameStateUpdateNotifies;

    public float EndMoveSoundDelay = 0.15f;

    private bool isMoveAnimationStarted;

    private bool isMoveAnimationFinished;

    private Vector3 moveTargetPosition = Vector3.zero;

    private Vector3 moveStartPosition = Vector3.zero;

    private bool isOffsetMove;

    public OnPawnMoveStateUpdate OnPawnMoveStateUpdateNotifies;

    private bool isRotateAnimationFinished;

    public OnPawnRotateStateUpdate OnPawnRotateStateUpdateNotifies;

    public OnPawnArrivalStateUpdate OnPawnArrivalStateUpdateNotifies;

    public OnPawnKillStateUpdate OnPawnKillStateUpdateNotifies;

    private bool isBeingKilled;

    private bool isKilledAnimationFinished;

    public OnPawnDeadStateUpdate OnPawnDeadStateUpdateNotifies;

    public OnPawnHeardNoise OnPawnHeardNoiseNotifies;

    private Vector3 startEular;

    private GameManager gameManager;
    private AudioManager audioManager;


    public PatrolPath PatrolPath
    {
        get
        {
            return patrolPath;
        }
        set
        {
            patrolPath = value;
        }
    }

    public bool JustWitnessedEnvironmentalKill
    {
        get
        {
            return justWitnessedEnvironmentalKill;
        }
        set
        {
            justWitnessedEnvironmentalKill = value;
        }
    }

    private void Start()
    {
        int num = Random.Range(0, MeshVariations.Length);
        for (int i = 0; i < MeshVariations.Length; i++)
        {
            if (num != i)
            {
                Destroy(MeshVariations[i]);
            }
            else
            {
                chosenMeshVariation = MeshVariations[i];
            }
        }
        num = Random.Range(0, MovingMeshVariations.Length);
        for (int j = 0; j < MovingMeshVariations.Length; j++)
        {
            if (num != j)
            {
                Destroy(MovingMeshVariations[j]);
                continue;
            }
            chosenMovingMeshVariation = MovingMeshVariations[j];
            chosenMovingMeshVariation.SetActive(false);
        }
    }

    protected override void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        audioManager = FindObjectOfType<AudioManager>();
        base.Awake();
        gameManager.RegisterAiPawn(this);
        fsm.SetDebugName(gameObject.name);
        fsm.Start(OnFirstFrameStateUpdate);
    }

    public bool IsEmoting(PawnEmotionType emotion)
    {
        return currentPawnEmotion != null && currentPawnEmotion.EmotionType == emotion;
    }

    public AiBehavior GetInitialBehavior()
    {
        return GetMovementBehavior(InitialBehavior);
    }

    private AiBehavior GetMovementBehavior(MovementBehavior movementBehavior)
    {
        switch (movementBehavior)
        {
            case MovementBehavior.Stationary:
                return new StationaryAiBehavior(this);
            case MovementBehavior.ForwardMoving:
                return new ForwardMovingAiBehavior(this);
            case MovementBehavior.ClockwiseRotating:
                return new RotatingAiBehavior(this, RotationSense.ClockWise, 1);
            case MovementBehavior.CounterClockwiseRotating:
                return new RotatingAiBehavior(this, RotationSense.CounterClockWise, 1);
            case MovementBehavior.HalfTurnRotating:
                return new RotatingAiBehavior(this, RotationSense.ClockWise, 2);
            case MovementBehavior.Partners:
                return new PartnersAiBehavior(this);
            case MovementBehavior.Sniper:
                return new SniperAiBehavior(this);
            case MovementBehavior.ChaseDog:
                return new ChaseDogAiBehavior(this);
            case MovementBehavior.Patrol:
                return new PatrolAiBehavior(this, patrolPath);
            case MovementBehavior.StrafingPartners:
                return new StrafingPartnersAiBehavior(this);
            case MovementBehavior._Unused:
                return null;
            default:
                return null;
        }
    }

    public void SetBehavior(AiBehavior aiBehavior)
    {
        if (currentBehavior != null)
        {
            currentBehavior.Deactivate();
        }
        currentBehavior = aiBehavior;
        aiBehavior.Activate();
    }

    public void UnsetBehavior()
    {
        if (currentBehavior != null)
        {
            currentBehavior.Deactivate();
        }
        currentBehavior = null;
    }

    public bool IsBehaviorActive<T>() where T : AiBehavior
    {
        T val = currentBehavior as T;

        if (val != null)
        {
            return true;
        }

        return false;
    }

    
    public virtual FSM.StateDelegate OnFirstFrameStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (OnPawnFirstFrameStateUpdateNotifies != null)
        {
            OnPawnFirstFrameStateUpdateNotifies(step, state);
        }
        if (step == FSM.Step.Update)
        {
            transform.position = CurrentNode.GetPositionForPawn(this);
            return OnIdleStateUpdate;
        }
        return null;
    }

    public virtual FSM.StateDelegate OnIdleStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        switch (step)
        {
            case FSM.Step.Enter:
                isLeavingIdle = false;
                initialPosition = transform.position;
                if (IsEmoting(PawnEmotionType.Exclamation))
                {
                    iTween.MoveBy(gameObject, iTween.Hash("y", ExclamatingJumpHeight, "time", ExclamatingJumpTime, "delay", ExclamatingWaitTime, "easetype", iTween.EaseType.easeOutQuad));
                    iTween.MoveTo(gameObject, iTween.Hash("position", initialPosition, "time", ExclamatingJumpTime, "delay", ExclamatingWaitTime + ExclamatingJumpTime, "easetype", iTween.EaseType.easeInQuad, "oncomplete", "OnExclamatingAnimationDone"));
                }
                initialRotation = transform.rotation;
                if (IsEmoting(PawnEmotionType.Question))
                {
                    isOddRotation = false;
                    iTween.RotateBy(gameObject, iTween.Hash("y", QuestionningTurnAngle / 360f, "time", QuestionningTurnTime, "delay", QuestionningWaitTime, "easetype", iTween.EaseType.easeOutQuad));
                    iTween.RotateTo(gameObject, iTween.Hash("rotation", initialRotation.eulerAngles, "time", QuestionningTurnTime, "delay", QuestionningWaitTime + QuestionningTurnTime, "easetype", iTween.EaseType.easeInQuad, "oncomplete", "OnQuestioningAnimationDone"));
                }
                return null;
            case FSM.Step.Leave:
                isLeavingIdle = true;
                iTween.Stop(gameObject);
                transform.position = initialPosition;
                transform.rotation = initialRotation;
                return null;
            case FSM.Step.Transition:
                return state;
            default:
                return null;
        }
    }

    public virtual FSM.StateDelegate OnDeclareMoveStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (step == FSM.Step.Enter)
        {
            currentBehavior.ExecuteMove();

            if (TargetNode != CurrentNode)
            {
                CurrentNode.OnPawnDeparture(this, barrier);
            }

            gameManager.Barrier.Remove(this);
            return OnIdleStateUpdate;
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

                    Node nodeInOrientation = CurrentNode.GetNodeInOrientation(nodeAttribute.Direction);
                    
                    SetTargetNode(nodeInOrientation);

                    if (TargetNode != CurrentNode)
                    {
                        CurrentNode.OnPawnDeparture(this, barrier);
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

    private void MoveAnimationIsFinished()
    {
        isMoveAnimationFinished = true;
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
            case "Snipe":
                animator.SetBool("Snipe", false);
                break;
            case "Kill":
                gameManager.PlayerPawn.StartKilledAnimation(this);
                break;
            case "Killed":
                isKilledAnimationFinished = true;
                break;
        }
    }

    public virtual FSM.StateDelegate OnMoveStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (OnPawnMoveStateUpdateNotifies != null)
        {
            OnPawnMoveStateUpdateNotifies(step, state);
        }
        switch (step)
        {
            case FSM.Step.Enter:
                {
                    gameManager.AiPawnSoundNotifications.Add(this);

                    
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
                    if (isOffsetMove)
                    {
                        MoveAddAnimation moveAddAnimation = new MoveAddAnimation(MoveOffsetTime, moveTargetPosition - transform.position);
                        moveAddAnimation.AnimationFinishedDelegate = MoveAnimationIsFinished;
                        Animateur.PushAnimation(gameObject, moveAddAnimation);
                        break;
                    }
                    PlayerPawn playerPawn = gameManager.PlayerPawn;
                    if (playerPawn.CurrentNode == TargetNode && IsPawnValidTarget(playerPawn))
                    {
                        animator.SetBool("KillMove", true);
                    }
                    else
                    {
                        animator.SetFloat("MoveAnimIndex", Random.Range(0, 5));
                        animator.SetBool("Move", true);
                    }
                    gameManager.AiPawnSoundNotificationsCount++;
                    break;
                }
            case FSM.Step.Update:
                if (!isOffsetMove && isMoveAnimationStarted)
                {
                    transform.position = Vector3.Lerp(moveStartPosition, moveTargetPosition, animator.GetFloat("MoveFactor"));
                }
                if (isMoveAnimationFinished && !animator.GetCurrentAnimatorStateInfo(0).IsName("Move"))
                {
                    isMoveAnimationFinished = false;
                    transform.position = moveTargetPosition;
                    SetCurrentNode(TargetNode);
                    currentBehavior.OnArrived(TargetNode);
                    gameManager.Barrier.Remove(this);
                    return OnIdleStateUpdate;
                }
                break;
        }
        return null;
    }

    private void RotateAnimationIsFinished()
    {
        isRotateAnimationFinished = true;
    }

    public virtual FSM.StateDelegate OnRotateStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (OnPawnRotateStateUpdateNotifies != null)
        {
            OnPawnRotateStateUpdateNotifies(step, state);
        }
        switch (step)
        {
            case FSM.Step.Enter:
                {
                    gameManager.AiPawnSoundNotifications.Add(this);
                    currentBehavior.ExecuteRotation();
                    RotatingAiBehavior rotatingAiBehavior = currentBehavior as RotatingAiBehavior;
                    if (TargetOrientation == Orientation.Invalid || (base.CurrentOrientation == base.TargetOrientation && rotatingAiBehavior == null))
                    {
                        gameManager.Barrier.Remove(this);
                        return OnIdleStateUpdate;
                    }
                    PartnersAiBehavior partnersAiBehavior = GetInitialBehavior() as PartnersAiBehavior;
                    if (partnersAiBehavior != null && (CurrentOrientation == TargetOrientation || CurrentOrientation.OppositeOrientation() == TargetOrientation))
                    {
                        gameManager.Barrier.Remove(this);
                        return OnIdleStateUpdate;
                    }

                    Quaternion quaternion = CurrentOrientation.OrientationToQuaternion();
                    Quaternion quaternion2 = TargetOrientation.OrientationToQuaternion();

                    float num = 0f;
                    if (rotatingAiBehavior == null)
                    {
                        float y = quaternion.eulerAngles.y;
                        float y2 = quaternion2.eulerAngles.y;
                        num = y2 - y;

                        if (num > 180f)
                        {
                            num -= 360f;
                        }
                        if (num < -180f)
                        {
                            num += 360f;
                        }
                    }
                    else
                    {
                        RotationSense rotationSense = rotatingAiBehavior.RotationSense;
                        float y3 = quaternion.eulerAngles.y;
                        float num2 = quaternion2.eulerAngles.y;
                        if (rotationSense == RotationSense.ClockWise)
                        {
                            if (y3 >= num2)
                            {
                                num2 += 360f;
                            }
                            num = num2 - y3;
                        }
                        else
                        {
                            if (num2 >= y3)
                            {
                                num2 -= 360f;
                            }
                            num = num2 - y3;
                        }
                    }
                    RotateAddAnimation rotateAddAnimation = new RotateAddAnimation(RotateTime, num * Vector3.up, RotateEaseType);
                    rotateAddAnimation.AnimationFinishedDelegate = RotateAnimationIsFinished;
                    Animateur.PushAnimation(gameObject, rotateAddAnimation);
                    gameManager.AiPawnSoundNotificationsCount++;
                    break;
                }
            case FSM.Step.Update:
                if (isRotateAnimationFinished)
                {
                    isRotateAnimationFinished = false;
                    transform.rotation = TargetOrientation.OrientationToQuaternion();
                    SetCurrentOrientation(TargetOrientation);
                    gameManager.Barrier.Remove(this);
                    return OnIdleStateUpdate;
                }
                break;
        }
        return null;
    }

    public virtual FSM.StateDelegate OnArrivalStateUpdate(FSM.Step step, FSM.StateDelegate state)
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

    private void OnArrived()
    {
        UnsetEmotion(true);
        AiBehavior initialBehavior = GetInitialBehavior();
        SetBehavior(initialBehavior);
    }

    public bool AnticipateKillNextMove()
    {
        Pawn pawn = currentBehavior.EvaluateKillTarget(currentBehavior.EvaluateNextNode());
        return pawn != null && IsPawnValidTarget(pawn);
    }

    public virtual FSM.StateDelegate OnKillStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (OnPawnKillStateUpdateNotifies != null)
        {
            OnPawnKillStateUpdateNotifies(step, state);
        }
        if (step == FSM.Step.Enter)
        {
            Pawn pawn = currentBehavior.EvaluateKillTarget();
            if (pawn != null && IsPawnValidTarget(pawn))
            {
                audioManager.PlaySoundOnceAmong(SoundConfig.KillSounds, SoundConfig.KillVolume);
                if (InitialBehavior == MovementBehavior.Sniper)
                {
                    animator.SetBool("Snipe", true);
                }
                if (pawn == gameManager.PlayerPawn)
                {
                    gameManager.PlayerPawn.OnKilled();
                }
            }
            gameManager.Barrier.Remove(this);
            return OnIdleStateUpdate;
        }
        return null;
    }

    public virtual void OnKilled()
    {
        SetTargetNode(null);
        isBeingKilled = true;
        UnsetEmotion(false);
    }

    public bool IsBeingKilled()
    {
        return isBeingKilled;
    }

    public void StartKilledAnimation(Pawn killer)
    {
        Quaternion rotation = base.MeshTransform.rotation;
        transform.rotation = killer.gameObject.transform.rotation;
        MeshTransform.rotation = rotation;
        animator.SetBool("Killed", true);
        int num = ((ForcedDeathAnimationIndex < 0) ? Random.Range(0, 3) : ForcedDeathAnimationIndex);
        animator.SetFloat("KilledAnimIndex", num);
    }

    public virtual FSM.StateDelegate OnDeadStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (OnPawnDeadStateUpdateNotifies != null)
        {
            OnPawnDeadStateUpdateNotifies(step, state);
        }
        switch (step)
        {
            case FSM.Step.Enter:
                {
                    isBeingKilled = false;
                    Cemetery cemetery = FindObjectOfType(typeof(Cemetery)) as Cemetery;

                    if (cemetery == null)
                    {
                        GameObject gameObject = new GameObject("Cemetery");
                        cemetery = gameObject.AddComponent<Cemetery>();
                    }

                    moveTargetPosition = cemetery.transform.position + cemetery.DeadPawns.Count * cemetery.Offset;
                    cemetery.DeadPawns.Add(this);
                    break;
                }
            case FSM.Step.Update:
                if (isKilledAnimationFinished)
                {
                    isKilledAnimationFinished = false;
                    base.transform.position = moveTargetPosition;
                    animator.SetBool("FallDown", true);
                    SetCurrentNode(null);
                    gameManager.OnAiPawnKilled(this);
                    gameManager.Barrier.Remove(this);
                }
                break;
        }
        return null;
    }

    public void OnNoise(Node node, Barrier barrier)
    {
        if (OnPawnHeardNoiseNotifies != null)
        {
            OnPawnHeardNoiseNotifies(node, barrier);
        }
        if (!fsm.IsStateActive(OnDeadStateUpdate))
        {
            SetEmotion(PawnEmotionType.Question);
            SetBehavior(new GotoNodeAiBehavior(this, node, OnArrived));
        }
    }

    public void OnEnvironmentalKill(Node node, Barrier barrier)
    {
        if (!fsm.IsStateActive(OnDeadStateUpdate))
        {
            justWitnessedEnvironmentalKill = true;
        }
    }

    public void OnPlayerPawnChangedDisguise()
    {
        currentBehavior.OnPlayerPawnChangedDisguise();
    }

    public void OnPlayerPawnUsedSecretPassage()
    {
        currentBehavior.OnPlayerPawnUsedSecretPassage();
    }

    public bool IsPawnInLineOfSight(Pawn pawn)
    {
        Node currentNode = pawn.CurrentNode;
        return CurrentNode.GetOrientationToNode(currentNode) == CurrentOrientation;
    }

    public override bool IsPawnValidTarget(Pawn pawn)
    {
        if (isPassive)
        {
            return false;
        }
        return base.IsPawnValidTarget(pawn);
    }

    public override bool IsDead()
    {
        return fsm.IsStateActive(OnDeadStateUpdate);
    }

    public void OnRebound()
    {
        startEular = transform.eulerAngles;
        Quaternion quaternion = default;
        Vector3 vector = gameManager.PlayerPawn.transform.position - base.CurrentNode.transform.position;
        vector.y = 0f;
        quaternion.SetLookRotation(vector.normalized);
        iTween.RotateTo(gameObject, iTween.Hash("time", 0.1f, "rotation", quaternion.eulerAngles, "easetype", iTween.EaseType.linear, "onComplete", "RotateComplete"));
        iTween.RotateTo(gameObject, iTween.Hash("time", 0.1f, "rotation", startEular, "delay", 1f, "easetype", iTween.EaseType.linear, "onComplete", "ReboundComplete", "onCompleteTarget", gameObject));
        gameManager.Barrier.Add(this);
    }

    private void ReboundComplete()
    {
        gameManager.Barrier.Remove(this);
    }

    public void SetEmotion(PawnEmotionType newEmotion)
    {
        PawnEmotion pawnEmotion = null;
        for (int i = 0; i < EmotionPrefabs.Count; i++)
        {
            if (EmotionPrefabs[i].EmotionType == newEmotion)
            {
                pawnEmotion = EmotionPrefabs[i];
            }
        }
        if (pawnEmotion == null)
        {
            Debug.Log("ERROR: Trying to set an emotion on an AIPawn that does not contain all emotion prefabs.");
            return;
        }
        if (newEmotion != PawnEmotionType.None)
        {
            UnsetEmotion(true);
        }
        currentPawnEmotion = pawnEmotion;
        emotionIconObject = Instantiate(pawnEmotion.gameObject, transform.position, Quaternion.identity);
        emotionIconObject.transform.parent = transform;
        if (newEmotion == PawnEmotionType.CantSee)
        {
            emotionIconObject.transform.localPosition = EmotionIconHeightEnd * Vector3.up;
        }
        else
        {
            emotionIconObject.transform.localPosition = EmotionIconHeightStart * Vector3.up;
            iTween.MoveTo(emotionIconObject, iTween.Hash("position", EmotionIconHeightEnd * Vector3.up, "time", EmotionTweenTime, "easetype", iTween.EaseType.easeOutBounce, "islocal", true));
        }
        if (dontKillIcon != null)
        {
            iTween.Stop(dontKillIcon);
            iTween.MoveTo(dontKillIcon, iTween.Hash("position", DontKillIconMaxHeight * Vector3.up, "time", EmotionTweenTime, "easetype", iTween.EaseType.easeOutBounce, "islocal", true));
        }
        if (newEmotion == PawnEmotionType.Question)
        {
            audioManager.PlaySoundOnceAmong(SoundConfig.OpenQuestionSounds, SoundConfig.OpenQuestionVolume);
        }
    }

    public void UnsetEmotion(bool playSound)
    {
        if (currentPawnEmotion != null && playSound && (currentPawnEmotion.EmotionType == PawnEmotionType.Exclamation || currentPawnEmotion.EmotionType == PawnEmotionType.Question))
        {
            audioManager.PlaySoundOnceAmong(SoundConfig.CloseQuestionSounds, SoundConfig.CloseQuestionVolume);
        }

        currentPawnEmotion = null;

        if (emotionIconObject != null)
        {
            Destroy(emotionIconObject);
        }
        if (dontKillIcon != null)
        {
            iTween.Stop(dontKillIcon);
            iTween.MoveTo(dontKillIcon, iTween.Hash("position", DontKillIconHeight * Vector3.up, "time", EmotionTweenTime, "easetype", iTween.EaseType.easeOutBounce, "islocal", true));
        }
    }

    public void SetMovingMeshActive(bool active)
    {
        if (active && chosenMovingMeshVariation != null)
        {
            chosenMovingMeshVariation.SetActive(true);
            chosenMeshVariation.SetActive(false);
        }
        else if (!active && chosenMovingMeshVariation != null)
        {
            chosenMovingMeshVariation.SetActive(false);
            chosenMeshVariation.SetActive(true);
        }
    }

    private void OnExclamatingAnimationDone()
    {
        if (!isLeavingIdle)
        {
            iTween.MoveBy(gameObject, iTween.Hash("y", ExclamatingJumpHeight, "time", ExclamatingJumpTime, "delay", ExclamatingWaitTime, "easetype", iTween.EaseType.linear));
            iTween.MoveTo(gameObject, iTween.Hash("position", initialPosition, "time", ExclamatingJumpTime, "delay", ExclamatingWaitTime + ExclamatingJumpTime, "easetype", iTween.EaseType.linear, "oncomplete", "OnExclamatingAnimationDone"));
        }
    }

    private void OnQuestioningAnimationDone()
    {
        if (!isLeavingIdle)
        {
            isOddRotation = !isOddRotation;
            float num = ((!isOddRotation) ? (QuestionningTurnAngle / 360f) : (0f - QuestionningTurnAngle / 360f));
            iTween.RotateBy(gameObject, iTween.Hash("y", num, "time", QuestionningTurnTime, "delay", QuestionningWaitTime, "easetype", iTween.EaseType.linear));
            iTween.RotateTo(gameObject, iTween.Hash("rotation", initialRotation.eulerAngles, "time", QuestionningTurnTime, "delay", QuestionningWaitTime + QuestionningTurnTime, "easetype", iTween.EaseType.linear, "oncomplete", "OnQuestioningAnimationDone"));
        }
    }
}
