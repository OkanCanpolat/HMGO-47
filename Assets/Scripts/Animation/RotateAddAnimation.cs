using UnityEngine;

public class RotateAddAnimation : BaseAnimation
{
    protected Vector3 deltaAngle;

    protected Interpolation.EasingFunction easingFunction;

    private Vector3 lastAngle = Vector3.zero;

    public RotateAddAnimation(float duration, Vector3 deltaAngle, Interpolation.EaseType easeType = Interpolation.EaseType.linear)
        : base(duration)
    {
        this.deltaAngle = deltaAngle;
        easingFunction = Interpolation.GetEasingFunction(easeType);
    }

    protected override void OnUpdate()
    {
        float num = easingFunction(0f, 1f, currentPhase);
        Vector3 vector = num * deltaAngle;
        DeltaRotation.eulerAngles = vector - lastAngle;
        lastAngle = vector;
    }
}
