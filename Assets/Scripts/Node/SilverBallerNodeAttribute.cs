using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SilverBallerNodeAttribute : NodeAttribute
{
    private enum State
    {
        Idle,
        StartAnim,
        StartWaitAnim,
        WaitAnim,
        StartRotate,
        Rotate,
        StartPause,
        Pause,
        Shoot,
        ShootPause,
        StartFinish,
        Finish
    }

    public SilverBallerSoundConfig SoundConfig;

    public float StartPauseDuration = 0.3f;

    public float RotateDuration = 0.2f;

    public float ShootPauseDuration = 0.1f;

    public float TransitAnimDuration = 0.2f;

    public int LoopCount = 3;

    public float RotationAngle = 25f;

    public Mesh[] SequencePoseMesh;

    private Barrier barrier;

    private int targetNodeIndex;

    private float startRotateTime;

    private float shootPauseTime;

    private float startTransitAnimTime;

    private float startPauseTime;

    private Orientation currentOrientation;

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
            case State.StartAnim:
                StartAnim();
                break;
            case State.StartWaitAnim:
                StartWaitAnim();
                break;
            case State.WaitAnim:
                WaitAnim();
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

    private void StartAnim()
    {
        faceCameraHorizontal = gameManager.PlayerPawn.GetComponentInChildren<FaceToCamera>();
        faceCameraHorizontal.enabled = false;
        currentOrientation = OrientationEnumMethods.ClosestOrientationFromTwoPositions(Vector3.zero, faceCameraHorizontal.transform.forward);
        pawnMeshFilter = faceCameraHorizontal.transform.GetChild(0).GetComponentInChildren<MeshFilter>();
        ChangePose();
        state = State.StartRotate;
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
            state = State.ShootPause;
        }
        else
        {
            float num = Mathf.PingPong((Time.timeSinceLevelLoad - startTransitAnimTime) / TransitAnimDuration, 1f / (float)LoopCount) - 1f / (float)LoopCount / 2f;
            identity.eulerAngles += ((targetNodeIndex != 1) ? new Vector3(num * RotationAngle, 0f, 0f) : new Vector3(0f, 0f, num * RotationAngle));
        }
        faceCameraHorizontal.transform.rotation = identity;
    }

    private void StartRotate()
    {
        startRotateTime = Time.timeSinceLevelLoad;
        startQuaternion = faceCameraHorizontal.transform.rotation;
        rotationTarget = currentOrientation.OrientationToQuaternion();
        state = State.Rotate;
    }

    private void ChangePose()
    {
        pawnMeshFilter.mesh = SequencePoseMesh[targetNodeIndex];
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

    private List<Node> GetInFrontNode()
    {
        List<Node> list = new List<Node>();
        for (int i = 0; i < gameManager.PlayerPawn.CurrentNode.AdjacentNodes.Count; i++)
        {
            Node node = gameManager.PlayerPawn.CurrentNode.AdjacentNodes[i];
            Vector3 lhs = ((targetNodeIndex != 0) ? faceCameraHorizontal.transform.forward : faceCameraHorizontal.transform.right);
            if (Mathf.Abs(Vector3.Dot(lhs, (node.transform.position - currentNode.transform.position).normalized)) > 0.9f)
            {
                list.Add(node);
            }
        }
        return list;
    }

    private void Shoot()
    {
        if (gameManager.PlayerPawn.IsPassive)
        {
            gameManager.OnPlayerBreakPassive();
        }
        List<Node> inFrontNode = GetInFrontNode();
        foreach (Node item in inFrontNode)
        {
            bool flag = false;
            List<AiPawn> list = new List<AiPawn>();
            foreach (Pawn pawn in item.Pawns)
            {
                AiPawn aiPawn = pawn as AiPawn;
                if (aiPawn != null)
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
            audioManager.PlaySoundOnceAmong(SoundConfig.ShootSounds, SoundConfig.ShootVolume);
            if (flag)
            {
                audioManager.PlaySoundOnceAmong(SoundConfig.ReboundSounds, SoundConfig.ReboundVolume);
                continue;
            }
            foreach (AiPawn item2 in list)
            {
                item2.StartKilledAnimation(gameManager.PlayerPawn);
                item2.OnKilled();
            }
        }
        targetNodeIndex++;
        currentOrientation = currentOrientation.NextOrientation(RotationSense.ClockWise);
        shootPauseTime = Time.timeSinceLevelLoad;
        gameManager.InteractHasKilled = true;
        state = State.StartWaitAnim;
    }

    private void DoNextShoot()
    {
        if (targetNodeIndex >= SequencePoseMesh.Length)
        {
            state = State.StartFinish;
            return;
        }
        ChangePose();
        state = State.Shoot;
    }

    private void ShootPause()
    {
        if (shootPauseTime + ShootPauseDuration < Time.timeSinceLevelLoad)
        {
            DoNextShoot();
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
        if (startRotateTime + RotateDuration < Time.timeSinceLevelLoad)
        {
            faceCameraHorizontal.transform.rotation = rotationTarget;
            gameManager.PlayerPawn.SetMesh(PlayerMeshType.Normal);
            faceCameraHorizontal.enabled = true;
            barrier.Remove(this);
            state = State.Idle;
        }
        else
        {
            float t = Interpolation.Interpolate(0f, 1f, (Time.timeSinceLevelLoad - startRotateTime) / RotateDuration, Interpolation.EaseType.easeInCubic);
            faceCameraHorizontal.transform.rotation = Quaternion.Lerp(startQuaternion, rotationTarget, t);
        }
    }

    public override void OnPlayerStartInteract(Barrier barrier)
    {
        if (gameObject.activeInHierarchy && enabled)
        {
            base.OnPlayerStartInteract(barrier);
            if (isActive)
            {
                isActive = false;
                audioManager.PlaySoundOnceAmong(SoundConfig.PickupSounds, SoundConfig.PickupVolume);
                HideAllMeshes();
                this.barrier = barrier;
                this.barrier.Add(this);
                state = State.StartAnim;
            }
        }
    }
}
