using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class CameraSphericalMovement : MonoBehaviour
{
    public Transform Center;

    public float Speed = 1f;

    private Camera cachedCamera;

    private Transform cachedTransform;

    private CameraPosition targetCameraPosition;

    private float ratio;

    private Vector3 startPosition = Vector3.zero;

    private Vector3 targetPosition = Vector3.zero;

    private Quaternion startRotation = Quaternion.identity;

    private Quaternion targetRotation = Quaternion.identity;

    private float startDistance;

    private float targetDistance;

    private float startFov;

    private float targetFov;

    private float startNear;

    private float targetNear;

    private float startFar;

    private float targetFar;

    private Vector2 targetOffset = Vector2.zero;

    private Vector2 currentOffset = Vector2.zero;

    private Vector2 scalingFactor = Vector2.zero;

    public List<CameraPosition> StartCameraPositions = new List<CameraPosition>();

    public float StartupAnimationDuration = 3f;

    public AudioClip MoveOutAudioClip;

    private bool isStartupAnimationActive;

    private int currentStartupAnimationCameraIndex;

    private CameraPosition savedCameraPosition;

    private bool isFirstUpdate = true;

    public CameraPosition TopCameraPosition;

    public Camera ChildHintCamera;

    private Vector3 pivotPosition;

    private Vector3 startPivotPosition;

    private Vector3 targetPivotPosition;

    private Vector2 proposedOffsetDelta = Vector2.zero;

    public bool IsVerticalPanningActive = true;

    public bool IsHorizontalPanningActive = true;

    private Vector3 noOffsetPosition = Vector3.zero;

    private Quaternion noOffsetRotation = Quaternion.identity;

    public float MaxVerticalAngle = 80f;

    public float MinVerticalAngle = 10f;

    public float HorizontalAngle = 45f;

    public float RotateSpeed = 4f;

    public float ComingBackSpeed = 1.5f;

    public float MovingSmoothTime = 0.2f;

    public float ComingBackSmoothTime = 0.5f;

    public float SpringTension = 12.5f;

    private Vector3 velocity = Vector3.zero;

    private bool isComingBack;

    [Inject] private AudioManager audioManager;
    public Vector2 TargetOffset
    {
        get
        {
            Vector2 offsett = targetOffset;
            offsett.x /= scalingFactor.x;
            offsett.y /= scalingFactor.y;
            return offsett;
        }
    }

    private void Start()
    {
        ratio = 1f;
        cachedCamera = GetComponent<Camera>();
        cachedTransform = transform;
        scalingFactor = new Vector2(2f * HorizontalAngle / (float)Screen.width, (MaxVerticalAngle - MinVerticalAngle) / (float)Screen.height);
        isFirstUpdate = true;

        if (StartCameraPositions.Count > 1)
        {
            IntroSoundClip introSoundClip = FindObjectOfType<IntroSoundClip>();

            if (introSoundClip != null)
            {
                audioManager.PlaySoundOnce(introSoundClip.Clip, introSoundClip.Volume);
            }
            CameraPosition cameraPosition = StartCameraPositions[0];
            cachedTransform.position = cameraPosition.transform.position;
            cachedTransform.rotation = cameraPosition.transform.rotation;
            cachedCamera.fieldOfView = cameraPosition.FieldOfView;
            cachedCamera.nearClipPlane = cameraPosition.NearClipPlane;
            cachedCamera.farClipPlane = cameraPosition.FarClipPlane;
            if (ChildHintCamera != null)
            {
                ChildHintCamera.fieldOfView = cachedCamera.fieldOfView;
                ChildHintCamera.nearClipPlane = cachedCamera.nearClipPlane;
                ChildHintCamera.farClipPlane = cachedCamera.farClipPlane;
            }
            isStartupAnimationActive = true;
        }
        noOffsetPosition = cachedTransform.position;
        noOffsetRotation = cachedTransform.rotation;
    }

    private float SpringRatio(float aCurrentValue, float aMaxValue, float aTension)
    {
        if (aCurrentValue < aMaxValue)
        {
            return 1f;
        }
        return Mathf.Pow(1f / (aCurrentValue / aMaxValue), aTension);
    }

    public void AddOffsetDelta(Vector2 a_OffsetDelta)
    {
        if (isComingBack)
        {
            targetOffset = currentOffset;
            isComingBack = false;
        }
        proposedOffsetDelta = a_OffsetDelta;
        float num = SpringRatio(Mathf.Abs(targetOffset.x), HorizontalAngle, SpringTension);
        proposedOffsetDelta.x *= num;
    }

    public void ResetOffset()
    {
        isComingBack = true;
        targetOffset = Vector2.zero;
    }

    public bool IsInAnimation()
    {
        return isStartupAnimationActive || ratio != 1f;
    }

    private Vector3 GetPivotPos(CameraPosition a_TargetCameraPosition)
    {
        return a_TargetCameraPosition.transform.position + Vector3.Project(Center.transform.position - a_TargetCameraPosition.transform.position, a_TargetCameraPosition.transform.forward);
    }

    private void Update()
    {
        if (Center == null)
        {
            return;
        }
        float num = Speed;
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            if (isStartupAnimationActive)
            {
                SetTarget(StartCameraPositions[1], true);
            }
        }
        if (isStartupAnimationActive)
        {
            num = 1f / (StartupAnimationDuration / (float)(StartCameraPositions.Count - 1));
            if (ratio >= 1f)
            {
                currentStartupAnimationCameraIndex++;
                if (currentStartupAnimationCameraIndex >= StartCameraPositions.Count)
                {
                    SetTarget(savedCameraPosition, true);
                    isStartupAnimationActive = false;
                    audioManager.PlaySoundOnce(MoveOutAudioClip, 1f);
                }
                else
                {
                    SetTarget(StartCameraPositions[currentStartupAnimationCameraIndex], true);
                }
            }
        }
        if (ratio < 1f)
        {
            ratio += num * Time.deltaTime;
            ratio = Mathf.Min(1f, ratio);
            float num2 = Interpolation.Interpolate(0f, 1f, ratio, Interpolation.EaseType.easeInOutQuad);
            pivotPosition = Vector3.Lerp(startPivotPosition, targetPivotPosition, num2);
            Vector3 normalized = (startPosition - pivotPosition).normalized;
            Vector3 normalized2 = (targetPosition - pivotPosition).normalized;
            float num3 = Vector3.Angle(normalized, normalized2);
            Vector3 normalized3 = Vector3.Cross(normalized, normalized2).normalized;
            Quaternion quaternion = Quaternion.AngleAxis(num2 * num3, normalized3);
            noOffsetPosition = quaternion * (startPosition - pivotPosition) + pivotPosition;
            float num4 = Mathf.Lerp(0f, targetDistance - startDistance, num2);
            noOffsetPosition += num4 * (noOffsetPosition - pivotPosition).normalized;
            noOffsetRotation = Quaternion.Slerp(startRotation, targetRotation, num2);
            cachedCamera.fieldOfView = Mathf.Lerp(startFov, targetFov, num2);
            cachedCamera.nearClipPlane = Mathf.Lerp(startNear, targetNear, num2);
            cachedCamera.farClipPlane = Mathf.Lerp(startFar, targetFar, num2);
            if (ChildHintCamera != null)
            {
                ChildHintCamera.fieldOfView = cachedCamera.fieldOfView;
                ChildHintCamera.nearClipPlane = cachedCamera.nearClipPlane;
                ChildHintCamera.farClipPlane = cachedCamera.farClipPlane;
            }
            proposedOffsetDelta = Vector2.zero;
            ResetOffset();
        }
        Vector2 vector = targetOffset + new Vector2(proposedOffsetDelta.x * scalingFactor.x, proposedOffsetDelta.y * scalingFactor.y);
        proposedOffsetDelta = Vector2.zero;
        targetOffset.x = vector.x;
        Vector3 a_PositionOut = Vector3.zero;
        Quaternion a_RotationOut = Quaternion.identity;
        ComputeVerticalOffset(noOffsetPosition, noOffsetRotation, 0f - vector.y, out a_PositionOut, out a_RotationOut);
        float num5 = Vector3.Angle(a_PositionOut - pivotPosition, Vector3.up);
        Vector3 lhs = a_PositionOut - pivotPosition;
        lhs.y = 0f;
        Vector3 rhs = noOffsetPosition - pivotPosition;
        rhs.y = 0f;
        float num6 = Vector3.Dot(lhs, rhs);
        if (num5 > 90f - MaxVerticalAngle && num5 < 90f - MinVerticalAngle && num6 >= 0f)
        {
            targetOffset.y = vector.y;
        }
        if (isComingBack)
        {
            currentOffset = Vector3.SmoothDamp(currentOffset, targetOffset, ref velocity, ComingBackSmoothTime);
        }
        else
        {
            currentOffset = Vector3.SmoothDamp(currentOffset, targetOffset, ref velocity, MovingSmoothTime);
        }
        Vector3 a_PositionOut2 = Vector3.zero;
        Quaternion a_RotationOut2 = Quaternion.identity;
        ComputeVerticalOffset(noOffsetPosition, noOffsetRotation, 0f - currentOffset.y, out a_PositionOut2, out a_RotationOut2);
        Quaternion quaternion2 = Quaternion.Euler(new Vector3(0f, (!IsHorizontalPanningActive) ? 0f : currentOffset.x, 0f));
        a_PositionOut2 = pivotPosition + quaternion2 * (a_PositionOut2 - pivotPosition);
        a_RotationOut2 = quaternion2 * a_RotationOut2;
        cachedTransform.position = a_PositionOut2;
        cachedTransform.rotation = a_RotationOut2;
    }

    public void StopStartAnimation()
    {
        if (isStartupAnimationActive)
        {
            isStartupAnimationActive = false;
            SetTarget(savedCameraPosition);
        }
    }

    private void ComputeVerticalOffset(Vector3 a_Position, Quaternion a_Rotation, float a_Offset, out Vector3 a_PositionOut, out Quaternion a_RotationOut)
    {
        Vector3 axis = a_Rotation * Vector3.right;
        Quaternion quaternion = Quaternion.AngleAxis((!IsVerticalPanningActive) ? 0f : a_Offset, axis);
        a_PositionOut = pivotPosition + quaternion * (a_Position - pivotPosition);
        a_RotationOut = quaternion * a_Rotation;
    }

    public void SetTarget(CameraPosition a_TargetCameraPosition, bool a_IsStartCamera = false)
    {
        if (!(Center == null) && !(a_TargetCameraPosition == null) && !(a_TargetCameraPosition == targetCameraPosition))
        {
            startPivotPosition = pivotPosition;
            targetPivotPosition = GetPivotPos(a_TargetCameraPosition);
            if (!a_IsStartCamera && isStartupAnimationActive)
            {
                savedCameraPosition = a_TargetCameraPosition;
                return;
            }
            targetCameraPosition = a_TargetCameraPosition;
            ratio = 0f;
            startPosition = noOffsetPosition;
            targetPosition = targetCameraPosition.transform.position;
            startRotation = noOffsetRotation;
            targetRotation = targetCameraPosition.transform.rotation;
            startDistance = Vector3.Distance(startPosition, targetPivotPosition);
            targetDistance = Vector3.Distance(targetPosition, targetPivotPosition);
            startFov = cachedCamera.fieldOfView;
            targetFov = a_TargetCameraPosition.FieldOfView;
            startNear = cachedCamera.nearClipPlane;
            targetNear = a_TargetCameraPosition.NearClipPlane;
            startFar = cachedCamera.farClipPlane;
            targetFar = a_TargetCameraPosition.FarClipPlane;
        }
    }
}
