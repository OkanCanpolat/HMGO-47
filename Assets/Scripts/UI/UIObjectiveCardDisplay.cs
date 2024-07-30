using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIObjectiveCardDisplay : UICardDisplay
{
    public Canvas Canvas;

    public UIObjectiveConfig ObjectiveConfig;

    public UIObjectiveCard ObjectiveCardPrefab;

    public float ShowObjectiveDelay = 0.3f;

    public float IntervalBetweenObjectives = 0.3f;

    public float StampStartDelay = 0.1f;

    public float StampScaleDuration = 0.3f;

    public float StampShakeDuration = 0.2f;

    public float StampFadeDuration = 0.3f;

    public Vector3 StampShakeAmount = new Vector3(0.05f, 0.05f, 0f);

    public Vector3 StampInitialScale = new Vector3(5f, 5f, 0f);

    private List<UIObjectiveCard> objectiveCards = new List<UIObjectiveCard>();

    public void Init(LevelDescription currentLevelDesc)
    {
        int maxStarsCount = currentLevelDesc.MaxStarCount;

        for (int i = 0; i < maxStarsCount; i++)
        {
            UIObjectiveCard uIObjectiveCard = CreateObjective(i);
            Image stampImage = uIObjectiveCard.StampImage;

            RectTransform transform = stampImage.transform as RectTransform;
            transform.localPosition += new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-15f, 15f));
            bool flag = !gameManager.StarObjectives[i].enabled;
            stampImage.enabled = flag;
            AddCard(uIObjectiveCard.gameObject);
            objectiveCards.Add(uIObjectiveCard);
        }
    }

    private UIObjectiveCard CreateObjective(int objectiveIndex)
    {
        Objective objective = gameManager.StarObjectives[objectiveIndex];
        UIObjectiveCard uIObjectiveCard = Instantiate(ObjectiveCardPrefab, Canvas.transform);
        uIObjectiveCard.gameObject.name = objectiveIndex + " - " + objective.GetType().ToString();
        UIObjectiveConfig.UIObjectiveInfo objectiveInfo = ObjectiveConfig.GetObjectiveInfo(objective.GetType());
        uIObjectiveCard.IconImage.sprite = objectiveInfo.Icon;
        uIObjectiveCard.NameLabel.text = objective.GetDesc();
        return uIObjectiveCard;
    }

    protected override IEnumerator ShowAllCards()
    {
        for (int objectiveIndex = 0; objectiveIndex < cards.Count; objectiveIndex++)
        {
            RotateCardForShow(objectiveIndex);
            yield return new WaitForSeconds(ShowOneCardAnimationLength);

            if (gameManager.StarObjectives[objectiveIndex].IsComplete())
            {
                yield return new WaitForSeconds(ShowObjectiveDelay);
                UIObjectiveCard objectiveCard = objectiveCards[objectiveIndex];
                Image stampImage = objectiveCard.StampImage;

                stampImage.enabled = true;
                iTween.ScaleFrom(stampImage.gameObject, iTween.Hash("scale", StampInitialScale, "time", StampScaleDuration, "easetype", iTween.EaseType.easeInExpo, "ignoretimescale", true));
                Color stampColor = stampImage.color;
                stampColor.a = 0;
                stampImage.color = stampColor;
                yield return StartCoroutine(FadeAlpha(stampImage, 0f, 1f, StampFadeDuration));
                iTween.ShakePosition(stampImage.gameObject, iTween.Hash("amount", StampShakeAmount, "time", StampShakeDuration, "ignoretimescale", true));
                audioManager.PlaySoundOnceAmong(SoundConfig.LevelResultsStampSounds, SoundConfig.LevelResultsStampVolume);
                yield return new WaitForSeconds(StampShakeDuration);

                yield return new WaitForSeconds(IntervalBetweenObjectives);
            }
        }
        state = CardDisplayState.ShowAnimationFinished;
    }

    private IEnumerator FadeAlpha(Image image, float from, float to, float duration)
    {
        float t = 0;
        Color color = image.color;
        color.a = from;
        image.color = color;

        while (t < 1)
        {
            float alpha = Mathf.Lerp(from, to, t);
            color.a = alpha;
            image.color = color;
            t += Time.deltaTime / duration;
            yield return null;
        }

        color.a = to;
        image.color = color;
    }
}
