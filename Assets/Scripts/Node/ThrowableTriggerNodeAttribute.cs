using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class ThrowableTriggerNodeAttribute : NodeAttribute
{
    public GameObject ThrowObjectPrefab;

    public float ThrowAnimationDuration = 2f;

    public float ThrowHeight = 4f;

    public float ThrowAnimationRotationSpeed = 2f;

    public Mesh PoseMesh;

    public float TransitAnimDuration = 0.5f;

    public int TransitLoopCount = 3;

    public float TransitRotationAngle = 25f;

    public Vector3 Offset = new Vector3(0.4f, 0.06f, 1.136f);

    private Transform throwObjectTransform;

    private Barrier barrier;

    private float currentThrowAnimationTime;

    private bool isThrowAnimationActive;

    private Vector3 throwEquationCoefficients = Vector3.zero;

    private float initialHorizontalVelocity;

    private Vector3 throwHorizontalAxis;

    private Node selectedNode;

    public SoundWave SoundWavePrefab;

    public ThrowableSoundConfig SoundConfig;

    private MeshFilter pawnMeshFilter;

    private FaceToCamera faceToCamera;

    private float startTransitAnimTime;

    private bool isTransitFoward;

    private Quaternion startQuaternion;

    private bool inTransit;

    private Vector3 startPosition;

    [Inject] private NodeManager nodeManager;
    [Inject] private AudioManager audioManager;
    private void UpdateTransitAnim()
    {
        if (inTransit)
        {
            if (startTransitAnimTime + TransitAnimDuration > Time.timeSinceLevelLoad)
            {
                TransitAnim();
                return;
            }

            faceToCamera.transform.rotation = startQuaternion;
            inTransit = false;
        }
    }

    private void StartTransitAnim(bool isTransitFoward)
    {
        startQuaternion = faceToCamera.transform.rotation;
        startTransitAnimTime = Time.timeSinceLevelLoad;
        this.isTransitFoward = isTransitFoward;
        inTransit = true;
    }

    private void TransitAnim()
    {
        Quaternion identity = Quaternion.identity;
        identity.eulerAngles = startQuaternion.eulerAngles;
        float num = Mathf.PingPong((Time.timeSinceLevelLoad - startTransitAnimTime) / TransitAnimDuration, 1f / TransitLoopCount) - 1f / TransitLoopCount / 2f;
        identity.eulerAngles += ((!isTransitFoward) ? new Vector3(0f, 0f, num * TransitRotationAngle) : new Vector3(num * TransitRotationAngle, 0f, 0f));
        faceToCamera.transform.rotation = identity;
    }

    private void Update()
    {
        UpdateTransitAnim();
        if (!isThrowAnimationActive)
        {
            return;
        }
        if (startTransitAnimTime + TransitAnimDuration < Time.timeSinceLevelLoad)
        {
            gameManager.PlayerPawn.SetMesh(PlayerMeshType.Normal);
        }
        if (currentThrowAnimationTime <= ThrowAnimationDuration)
        {
            throwObjectTransform.position = startPosition;
            float num = initialHorizontalVelocity * currentThrowAnimationTime;
            throwObjectTransform.position += num * throwHorizontalAxis;
            float num2 = throwEquationCoefficients.x * num * num + throwEquationCoefficients.y * num + throwEquationCoefficients.z;
            throwObjectTransform.position += num2 * Vector3.up;
            float angle = 360f * ThrowAnimationRotationSpeed * currentThrowAnimationTime / ThrowAnimationDuration;
            throwObjectTransform.rotation = Quaternion.AngleAxis(angle, Vector3.Cross(Vector3.up, throwHorizontalAxis));
            currentThrowAnimationTime += Time.deltaTime;
            return;
        }

        isThrowAnimationActive = false;
        currentThrowAnimationTime = 0f;
        Destroy(throwObjectTransform.gameObject);
        audioManager.PlaySoundOnceAmong(SoundConfig.LandingSounds, SoundConfig.LandingVolume);
        audioManager.PlaySoundOnceAmong(SoundConfig.RadiusSounds, SoundConfig.RadiusVolume);
        SoundWave soundWave = Instantiate(SoundWavePrefab);
        if (soundWave != null)
        {
            soundWave.BeginAnimation(selectedNode, barrier);
            barrier.Remove(this);
            faceToCamera.enabled = true;
            gameManager.PlayerPawn.SetMesh(PlayerMeshType.Normal);
        }
    }

    public override void OnPlayerStartInteract(Barrier barrier)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        base.OnPlayerStartInteract(barrier);
        if (isActive)
        {
            gameManager.ShouldSelectNodeForInteraction = true;
            isActive = false;
            HideAllMeshes();
            audioManager.PlaySoundOnceAmong(SoundConfig.PickupSounds, SoundConfig.PickupVolume);
            faceToCamera = gameManager.PlayerPawn.GetComponentInChildren<FaceToCamera>();
            pawnMeshFilter = faceToCamera.transform.GetChild(0).GetComponentInChildren<MeshFilter>();
            gameManager.PlayerPawn.SetMesh(PlayerMeshType.Throwing);
            faceToCamera.enabled = false;
            StartTransitAnim(true);
            GameObject gameObject = Instantiate(ThrowObjectPrefab, pawnMeshFilter.transform.position, Quaternion.identity);
            if (gameObject != null)
            {
                throwObjectTransform = gameObject.transform;
                throwObjectTransform.parent = pawnMeshFilter.transform;
                throwObjectTransform.localPosition = Offset;
                startPosition = throwObjectTransform.position;
            }
        }
    }

    public override void OnPlayerInteract(Node selectedNode, Barrier barrier)
    {
        if (gameObject.activeInHierarchy)
        {
            base.OnPlayerInteract(selectedNode, barrier);
            Debug.Log(selectedNode.name);
            
            this.barrier = barrier;
            this.barrier.Add(this);
            isThrowAnimationActive = true;
            StartTransitAnim(false);
            Vector3 vector = selectedNode.transform.position - startPosition;
            Vector3 vector2 = vector;
            vector2.y = 0f;
            float magnitude = vector2.magnitude;
            float y = vector.y;
            throwHorizontalAxis = vector2.normalized;
            initialHorizontalVelocity = magnitude / ThrowAnimationDuration;
            Vector2 a_Point = new Vector2(0f, 0f);
            Vector2 a_Point2 = new Vector2(magnitude, y);
            throwEquationCoefficients = FindQuadraticThrough3Points(a_Point1: new Vector2(0.5f * (a_Point.x + a_Point2.x), ThrowHeight), a_Point0: a_Point, a_Point2: a_Point2);
            this.selectedNode = selectedNode;
            audioManager.PlaySoundOnceAmong(SoundConfig.ThrowSounds, SoundConfig.ThrowVolume);
        }
    }

    private Vector3 FindQuadraticThrough3Points(Vector2 a_Point0, Vector2 a_Point1, Vector2 a_Point2)
    {
        float num = (a_Point1.y - a_Point0.y) * (a_Point0.x - a_Point2.x) + (a_Point2.y - a_Point0.y) * (a_Point1.x - a_Point0.x);
        float num2 = (a_Point0.x - a_Point2.x) * (a_Point1.x * a_Point1.x - a_Point0.x * a_Point0.x) + (a_Point1.x - a_Point0.x) * (a_Point2.x * a_Point2.x - a_Point0.x * a_Point0.x);
        float num3 = num / num2;
        float num4 = (a_Point1.y - a_Point0.y - num3 * (a_Point1.x * a_Point1.x - a_Point0.x * a_Point0.x)) / (a_Point1.x - a_Point0.x);
        float z = a_Point0.y - num3 * a_Point0.x * a_Point0.x - num4 * a_Point0.x;
        return new Vector3(num3, num4, z);
    }

    public override void OnEnterNodeSelection(bool shouldSelectNodeForInteraction)
    {
        if (!gameObject.activeInHierarchy || !shouldSelectNodeForInteraction)
        {
            return;
        }


        foreach (Node node in nodeManager.Nodes)
        {
            bool flag = true;
            if (currentNode != node && node.IsInCrossDistanceFrom(currentNode, 1.1f * nodeManager.Distance))
            {
                foreach (Pawn pawn in node.Pawns)
                {
                    if (!pawn.IsDead())
                    {
                        flag = false;
                        break;
                    }
                }
            }
            else
            {
                flag = false;
            }
            if (flag)
            {
                node.DisplayIndicator(true);
            }
        }
    }
}
