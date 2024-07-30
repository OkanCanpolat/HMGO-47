using System;
using UnityEngine;

public class FSM 
{
    public enum Step
    {
        Enter,
        Update,
        Leave,
        Transition
    }

    public delegate StateDelegate StateDelegate(Step step, StateDelegate stateDelegate);

    private StateDelegate currentStateDelegate;
    private StateDelegate requestedStateDelegate;

    private bool retryRequestUntilItWorks;

    private string debugName;
    private bool isDebugActivated;

    private StateDelegate CurrentStateDelegate
    {
        get
        {
            return currentStateDelegate;
        }
        set
        {
            currentStateDelegate = value;
        }
    }

    public void SetDebugName(string name)
    {
        debugName = name;
    }

    public void SetDebugActivation(bool debugActivation)
    {
        isDebugActivated = debugActivation;
    }

    public void Start(StateDelegate firstStateDelegate)
    {
        TransitionTo(firstStateDelegate);
        requestedStateDelegate = null;
        retryRequestUntilItWorks = false;
    }

    public void Update()
    {
        if (CurrentStateDelegate == null)
        {
            return;
        }

        if (requestedStateDelegate != null)
        {
            StateDelegate stateDelegate = CurrentStateDelegate(Step.Transition, requestedStateDelegate);
            if (stateDelegate != null)
            {
                TransitionTo(stateDelegate);
            }
            else if (isDebugActivated)
            {
                Debug.Log(Time.frameCount + " Request to " + requestedStateDelegate.Method.Name + " has been dismissed by " + CurrentStateDelegate.Method.Name + ((!retryRequestUntilItWorks) ? string.Empty : " (will retry)"));
            }
            if (!retryRequestUntilItWorks)
            {
                requestedStateDelegate = null;
            }
        }
        StateDelegate stateDelegate2 = CurrentStateDelegate(Step.Update, CurrentStateDelegate);
        if (stateDelegate2 != null)
        {
            TransitionTo(stateDelegate2);
        }
    }

    public void Stop()
    {
        if (CurrentStateDelegate != null)
        {
            CurrentStateDelegate(Step.Leave, null);
        }
    }

    public void ForceTransition(StateDelegate stateDelegate)
    {
        TransitionTo(stateDelegate);
    }

    public void RequestState(StateDelegate requestedStateDelegate, bool retryRequestUntilItWorks = false)
    {
        if (this.requestedStateDelegate != null && isDebugActivated)
        {
            Debug.Log(Time.frameCount + " Requesting for " + requestedStateDelegate.Method.Name + " but a request for " + this.requestedStateDelegate.Method.Name + "is already active");
        }

        this.requestedStateDelegate = requestedStateDelegate;
        this.retryRequestUntilItWorks = retryRequestUntilItWorks;
    }

    public bool IsStateActive(StateDelegate stateDelegate)
    {
        return CurrentStateDelegate == stateDelegate;
    }

    private void TransitionTo(StateDelegate nextStateDelegate)
    {
        StateDelegate currentStateDelegate = CurrentStateDelegate;

        if (isDebugActivated)
        {
            Debug.Log(Time.frameCount + " (" + debugName + "): Leaving state " + currentStateDelegate.Method.Name + " for state " + nextStateDelegate.Method.Name);
        }

        if (CurrentStateDelegate != null)
        {
            CurrentStateDelegate(Step.Leave, nextStateDelegate);
        }


        CurrentStateDelegate = nextStateDelegate;

        StateDelegate stateDelegate = CurrentStateDelegate(Step.Enter, currentStateDelegate);
        if (stateDelegate != null)
        {
            TransitionTo(stateDelegate);
        }
    }

    internal void SetDebugActivation(object m_IsFsmDebugActivated)
    {
        throw new NotImplementedException();
    }
}
