using UnityEngine;

public class MoveAddAnimation : BaseAnimation
{
    protected Vector3 deltaMotion;

    protected Interpolation.EasingFunction easingFunction;

    private Vector3 lastPosition = Vector3.zero;

    public MoveAddAnimation(float duration, Vector3 deltaMotion, Interpolation.EaseType easeType = Interpolation.EaseType.linear)
        : base(duration)
    {
        this.deltaMotion = deltaMotion;
        easingFunction = Interpolation.GetEasingFunction(easeType);
    }

    protected override void OnUpdate()
    {
        float num = easingFunction(0f, 1f, currentPhase);
        Vector3 vector = num * deltaMotion;
        DeltaPosition = vector - lastPosition;
        lastPosition = vector;
    }
}
