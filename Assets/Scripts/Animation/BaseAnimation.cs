using UnityEngine;


public delegate void AnimationFinished();
public class BaseAnimation
{
    public float Delay;

    public AnimationFinished AnimationFinishedDelegate;

    public Animateur Animateur;

    public Vector3 DeltaPosition = Vector3.zero;

    public Quaternion DeltaRotation = Quaternion.identity;

    protected float Duration = 1f;

    protected float currentTime;

    protected float currentPhase;

    public BaseAnimation(float a_Duration)
    {
        Duration = a_Duration;
    }

    protected virtual void OnStart()
    {
    }

    public void Start()
    {
        currentTime = 0f;
        currentPhase = 0f;
        OnStart();
    }

    protected virtual void OnUpdate()
    {
    }

    public virtual void Update()
    {
        float elapsedTime = Time.deltaTime;

        if (Delay > 0f)
        {
            Delay -= elapsedTime;

            if (!(Delay < 0f))
            {
                return;
            }

            elapsedTime = 0f - Delay;
            Delay = 0f;
        }

        currentTime += elapsedTime;
        DeltaPosition = Vector3.zero;
        DeltaRotation = Quaternion.identity;

        if (currentTime > Duration)
        {
            currentTime = Duration;
            Animateur.finishedAnimations.Add(this);
        }
        if (Duration == 0f)
        {
            currentPhase = 1f;
        }
        else
        {
            currentPhase = currentTime / Duration;
        }
        OnUpdate();
    }

    protected virtual void OnStop()
    {
    }

    public virtual void Stop()
    {
        OnStop();
        if (AnimationFinishedDelegate != null)
        {
            AnimationFinishedDelegate();
        }
    }
}
