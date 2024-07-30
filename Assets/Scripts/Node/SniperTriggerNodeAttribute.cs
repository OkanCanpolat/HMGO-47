using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SniperTriggerNodeAttribute : NodeAttribute
{
    private enum State
    {
        Idle,
        StartRotate,
        Rotate,
        StartPause,
        Pause,
        Shoot,
        StartWaitAnim,
        WaitAnim,
        StartShootPause,
        ShootPause,
        StartFinish,
        Finish
    }

    public List<Node> TargetNodes = new List<Node>();

    public SniperSoundConfig m_SoundConfig;

    public Mesh PoseMesh;

    private float rotationAngle = 15f;

    public float StartPauseDuration = 0.3f;

    public float RotateDuration = 0.2f;

    public float FinishRotateDuration = 0.2f;

    public float ShootPauseDuration = 0.1f;

    public float TransitAnimDuration = 0.2f;

    public int LoopCount = 3;

    private Node selectedNode;

    private Barrier barrier;

    private float startRotateTime;

    private float shootPauseTime;

    private float startPauseTime;

    private float startTransitAnimTime;

    private Quaternion rotationTarget;

    private Quaternion startQuaternion;

    private FaceToCamera faceCameraHorizontal;

    private MeshFilter pawnMeshFilter;

    private State state;

    [Inject] private AudioManager audioManager;

    private void Update()
    {
        switch (state)
        {
            case State.Idle:
                break;
            case State.StartRotate:
                StartRotate();
                break;
            case State.Rotate:
                Rotate();
                break;
            case State.StartPause:
                StartPause();
                break;
            case State.Pause:
                Pause();
                break;
            case State.Shoot:
                Shoot();
                break;
            case State.StartWaitAnim:
                StartWaitAnim();
                break;
            case State.WaitAnim:
                WaitAnim();
                break;
            case State.StartShootPause:
                StartShootPause();
                break;
            case State.ShootPause:
                ShootPause();
                break;
            case State.StartFinish:
                StartFinish();
                break;
            case State.Finish:
                Finish();
                break;
        }
    }

    private void StartRotate()
    {
        startRotateTime = Time.timeSinceLevelLoad;
        startQuaternion = faceCameraHorizontal.transform.rotation;
        rotationTarget = default;
        Vector3 vector = selectedNode.transform.position - currentNode.transform.position;
        vector.y = 0f;
        rotationTarget.SetLookRotation(vector.normalized);
        state = State.Rotate;
    }

    private void Rotate()
    {
        if (startRotateTime + RotateDuration < Time.timeSinceLevelLoad)
        {
            faceCameraHorizontal.transform.rotation = rotationTarget;
            state = State.StartPause;
        }
        else
        {
            faceCameraHorizontal.transform.rotation = Quaternion.Lerp(startQuaternion, rotationTarget, (Time.timeSinceLevelLoad - startRotateTime) / RotateDuration);
        }
    }

    private void StartPause()
    {
        startPauseTime = Time.timeSinceLevelLoad;
        state = State.Pause;
    }

    private void Pause()
    {
        if (startPauseTime + StartPauseDuration < Time.timeSinceLevelLoad)
        {
            state = State.Shoot;
        }
    }

    private void Shoot()
    {
        selectedNode.OnPlayerSniperShot(barrier);
        bool flag = false;
        List<AiPawn> list = new List<AiPawn>();

        foreach (AiPawn aiPawn in gameManager.AiPawns)
        {
            if (aiPawn != null && aiPawn.CurrentNode == selectedNode && !aiPawn.IsDead())
            {
                if (!aiPawn.HasShield)
                {
                    list.Add(aiPawn);
                    continue;
                }
                flag = true;
                aiPawn.OnRebound();
            }
        }
        audioManager.PlaySoundOnceAmong(m_SoundConfig.ShootSounds, m_SoundConfig.ShootVolume);
        if (flag)
        {
            audioManager.PlaySoundOnceAmong(m_SoundConfig.ReboundSounds, m_SoundConfig.ReboundVolume);
        }
        else if (list.Count != 0)
        {
            foreach (AiPawn item in list)
            {
                item.StartKilledAnimation(gameManager.PlayerPawn);
                item.OnKilled();
            }
            gameManager.InteractHasKilled = true;
        }
        state = State.StartWaitAnim;
    }

    private void StartWaitAnim()
    {
        startQuaternion = faceCameraHorizontal.transform.rotation;
        startTransitAnimTime = Time.timeSinceLevelLoad;
        state = State.WaitAnim;
    }

    private void WaitAnim()
    {
        Quaternion identity = Quaternion.identity;
        identity.eulerAngles = startQuaternion.eulerAngles;
        if (startTransitAnimTime + TransitAnimDuration < Time.timeSinceLevelLoad)
        {
            state = State.StartShootPause;
        }
        else
        {
            float num = Time.timeSinceLevelLoad - startTransitAnimTime;
            float num2 = TransitAnimDuration / LoopCount;
            float num3 = num / num2;
            float num4 = 0f - Mathf.Sin(num3 * Mathf.PI * 2f);
            identity.eulerAngles += new Vector3(num4 * rotationAngle, 0f, 0f);
        }
        faceCameraHorizontal.transform.rotation = identity;
    }

    private void StartShootPause()
    {
        shootPauseTime = Time.timeSinceLevelLoad;
        state = State.ShootPause;
    }

    private void ShootPause()
    {
        if (shootPauseTime + ShootPauseDuration < Time.timeSinceLevelLoad)
        {
            state = State.StartFinish;
        }
    }

    private void StartFinish()
    {
        startRotateTime = Time.timeSinceLevelLoad;
        startQuaternion = faceCameraHorizontal.transform.rotation;
        faceCameraHorizontal.FaceCamera();
        rotationTarget = faceCameraHorizontal.transform.rotation;
        faceCameraHorizontal.transform.rotation = startQuaternion;
        state = State.Finish;
    }

    private void Finish()
    {
        if (startRotateTime + FinishRotateDuration < Time.timeSinceLevelLoad)
        {
            faceCameraHorizontal.transform.rotation = rotationTarget;
            gameManager.PlayerPawn.SetMesh(PlayerMeshType.Normal);
            faceCameraHorizontal.enabled = true;
            barrier.Remove(this);
            state = State.Idle;
        }
        else
        {
            float t = Interpolation.Interpolate(0f, 1f, (Time.timeSinceLevelLoad - startRotateTime) / FinishRotateDuration, Interpolation.EaseType.easeInCubic);
            faceCameraHorizontal.transform.rotation = Quaternion.Lerp(startQuaternion, rotationTarget, t);
        }
    }

    public override void OnPlayerStartInteract(Barrier a_Barrier)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        base.OnPlayerStartInteract(a_Barrier);

        if (isActive)
        {
            if (gameManager.IsPassive)
            {
                gameManager.OnPlayerBreakPassive();
            }
            gameManager.ShouldSelectNodeForInteraction = true;
            isActive = false;
            HideAllMeshes();
            audioManager.PlaySoundOnceAmong(m_SoundConfig.PickupSounds, m_SoundConfig.PickupVolume);
            faceCameraHorizontal = gameManager.PlayerPawn.GetComponentInChildren<FaceToCamera>();
            pawnMeshFilter = faceCameraHorizontal.transform.GetChild(0).GetComponentInChildren<MeshFilter>();
            pawnMeshFilter.mesh = PoseMesh;
        }
    }

    public override void OnPlayerInteract(Node selectedNode, Barrier barrier)
    {
        if (gameObject.activeInHierarchy)
        {
            base.OnPlayerInteract(selectedNode, barrier);
            this.selectedNode = selectedNode;
            this.barrier = barrier;
            this.barrier.Add(this);
            faceCameraHorizontal.enabled = false;
            state = State.StartRotate;
        }
    }

    public override void OnEnterNodeSelection(bool shouldSelectNodeForInteraction)
    {
        if (!gameObject.activeInHierarchy || !shouldSelectNodeForInteraction)
        {
            return;
        }
        List<Node> list = new List<Node>();
        foreach (Node targetNode in TargetNodes)
        {
            if (targetNode != null)
            {
                targetNode.DisplayIndicator(true);
                list.Add(targetNode);
            }
        }
        list.Clear();
    }

    public override void OnPlayerPawnKilled()
    {
        if (pawnMeshFilter != null)
        {
            gameManager.PlayerPawn.SetMesh(PlayerMeshType.Normal);
        }
    }
}
