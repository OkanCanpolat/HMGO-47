using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class LevelCompleteViewController : ViewController
{
    public UIObjectiveCardDisplay CardDisplay;

    public Transform NextButton;

    public Transform RestartButton;

    public Transform ButtonContainer;

    public TMP_Text LevelNameLabel;

    public Transform TitleTransform;

    public float TitleGoDownDuration = 0.5f;

    public float ButtonAnimationHeightOffset = 585f;

    public float ShowButtonsLevelDuration = 0.5f;

    private PostProcessVolume postProcessingVolume;

    private float blurEffectLength = 0.2f;

    private float blurEffectTarget;

    private Vector3 buttonContainerInitialPosition;

    private GameManager gameManager;

    private DepthOfField depthOfField;


    public void Awake()
    {
        postProcessingVolume = Camera.main.GetComponentInChildren<PostProcessVolume>();

        if (!postProcessingVolume.profile.TryGetSettings(out depthOfField))
        {
            Debug.LogError("PP Effects Not Found");
        }
        gameManager = FindObjectOfType<GameManager>();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        buttonContainerInitialPosition = ButtonContainer.transform.localPosition;

        if ((bool)postProcessingVolume)
        {
            depthOfField.active = true;
            depthOfField.enabled.value = true;
            depthOfField.focalLength.overrideState = true;
            depthOfField.focalLength.value = 0f;
            blurEffectTarget = 300f;
            StartCoroutine(StartBlurEffect(depthOfField.focalLength.value, blurEffectTarget));
        }
    }
    public override void OnLeave()
    {
        if ((bool)postProcessingVolume)
        {
            blurEffectTarget = 0f;
            postProcessingVolume.enabled = false;
        }
        base.OnLeave();
    }
    public void Start()
    {
        LevelDescription currentLevelDesc = gameManager.LevelDescription;
        LevelNameLabel.text = "LEVEL " + currentLevelDesc.Chapter + "-" + currentLevelDesc.Level + " COMPLETE";

        if (currentLevelDesc != null)
        {
            CardDisplay.Init(currentLevelDesc);
            StartAnimation();
        }
    }
    private IEnumerator StartBlurEffect(float from, float to)
    {
        float t = 0;
        depthOfField.focalLength.overrideState = true;

        while (t < 1)
        {
            float blur = Mathf.Lerp(from, to, t);
            depthOfField.focalLength.value = blur;
            t += Time.deltaTime / blurEffectLength;
            yield return null;
        }

        depthOfField.focalLength.value = to;
    }

    private void StartAnimation()
    {
        iTween.MoveFrom(TitleTransform.gameObject, iTween.Hash("position", new Vector3(0, 750, 0), "time", TitleGoDownDuration, "islocal", true, "easetype", iTween.EaseType.linear));
        ButtonContainer.transform.localPosition = buttonContainerInitialPosition + new Vector3(0f, 0f - ButtonAnimationHeightOffset, 0f);

        if ((bool)NextButton)
        {
            NextButton.gameObject.SetActive(false);
        }
        if ((bool)RestartButton)
        {
            RestartButton.gameObject.SetActive(false);
        }
        CardDisplay.ShowAllCards(FinishAnimation);
    }

    private void FinishAnimation()
    {
        if ((bool)NextButton)
        {
            NextButton.gameObject.SetActive(true);
        }
        if ((bool)RestartButton)
        {
            RestartButton.gameObject.SetActive(true);
        }

        iTween.MoveTo(ButtonContainer.gameObject, iTween.Hash("position", buttonContainerInitialPosition, "time", ShowButtonsLevelDuration, "islocal", true, "easetype", iTween.EaseType.linear));
    }
}
