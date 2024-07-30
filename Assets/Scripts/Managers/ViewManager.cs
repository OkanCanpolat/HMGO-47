using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ViewManager : MonoBehaviour
{
    public UiSoundConfig SoundConfig;
    public float AlphaSpeed = 2f;
    public LevelCompleteViewController LevelCompleteViewControllerPrefab;
    public GameViewController GameViewControllerPrefab;
    private FSM fsm = new FSM();
    private bool isFsmDebugActivated;
    public Image FadeImage;
    private ViewController currentViewController;

    [Inject] private AudioManager audioManager;
    private void Update()
    {
        fsm.SetDebugActivation(isFsmDebugActivated);
        fsm.Update();
    }

    private void Start()
    {
        fsm.SetDebugName(gameObject.name);
        fsm.Start(OnFirstFrameStateUpdate);
    }

    private FSM.StateDelegate OnFirstFrameStateUpdate(FSM.Step step, FSM.StateDelegate state)
    {
        if (step == FSM.Step.Update)
        {
            if (currentViewController == null)
            {
                RequestViewController(GameViewControllerPrefab);
            }
            return OnFadeInStateUpdate;
        }
        return null;
    }

    private FSM.StateDelegate OnFadeInStateUpdate(FSM.Step a_Step, FSM.StateDelegate a_State)
    {
        Color color = FadeImage.color;

        switch (a_Step)
        {
            case FSM.Step.Enter:
                color.a = 1f;
                FadeImage.color = color;
                FadeImage.gameObject.SetActive(true);
                audioManager.PlaySoundOnce(SoundConfig.IntroSound, SoundConfig.IntroVolume);

                break;
            case FSM.Step.Update:
                {
                    float alpha = color.a - AlphaSpeed * Time.deltaTime;

                    if (alpha >= 0f)
                    {
                        color.a = alpha;
                        FadeImage.color = color;
                        break;
                    }

                    color.a = 0;
                    FadeImage.color = color;
                    FadeImage.gameObject.SetActive(false);
                    return OnIdleStateUpdate;
                }
        }
        return null;
    }
    private FSM.StateDelegate OnIdleStateUpdate(FSM.Step a_Step, FSM.StateDelegate a_State)
    {
        if (a_Step == FSM.Step.Transition)
        {
            return a_State;
        }
        return null;
    }

    public void RequestViewController(ViewController controller)
    {
        DestroyCurrentViewController();
        LoadNewViewController(controller);
    }
    public void NotifyLevelComplete()
    {
        RequestViewController(LevelCompleteViewControllerPrefab);
    }

    private void DestroyCurrentViewController()
    {
        if ((bool)currentViewController)
        {
            currentViewController.OnLeave();

            Destroy(currentViewController.gameObject);

            currentViewController = null;
        }
    }

    private void LoadNewViewController(ViewController viewController)
    {
        ViewController controller = Instantiate(viewController, transform);
        controller.transform.position = Vector3.zero;
        controller.transform.rotation = Quaternion.identity;
        controller.transform.localScale = Vector3.one;
        currentViewController = controller;

        if ((bool)currentViewController)
        {
            currentViewController.ViewManager = this;
            currentViewController.OnEnter();
        }
    }

    public void NotifyLevelObjectivesCompleted()
    {
        audioManager.PlaySoundOnce(SoundConfig.LevelCompleteSound, SoundConfig.LevelCompleteVolume);
    }
    public void NotifyEnterGame()
    {
        AmbientMusic ambientMusic = FindObjectOfType<AmbientMusic>();

        if (ambientMusic != null)
        {
            audioManager.FadeIn(1, ambientMusic.Clip, false);
        }
        else
        {
            audioManager.FadeOut(1);
        }
    }

}
