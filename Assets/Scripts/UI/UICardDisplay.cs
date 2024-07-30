using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICardDisplay : MonoBehaviour
{
    public enum CardDisplayState
    {
        Uninitialized,
        Initialized,
        ShowAnimation,
        ShowAnimationFinished,
        Active,
        HideAnimation,
        HideAnimationFinished
    }

    public delegate void CardDisplayStateEventListener();

    public float CardYOffset = 4000f;

    public float CardYOffsetRandomRange = 40f;

    public float CardSpacingAngle = 2f;

    public float ShowOneCardAnimationLength = 0.3f;

    public float HideAllCardsAnimationLength = 0.3f;

    public UiSoundConfig SoundConfig;

    protected CardDisplayState state;

    protected CardDisplayStateEventListener notifyShowListeners;

    protected CardDisplayStateEventListener notifyHideListeners;

    protected List<GameObject> cards = new List<GameObject>();

    protected AudioManager audioManager;
    protected GameManager gameManager;

    public int CardCount
    {
        get
        {
            return cards.Count;
        }
    }

    public CardDisplayState State
    {
        get
        {
            return state;
        }
    }

    virtual protected void Awake()
    {
        audioManager = FindObjectOfType<AudioManager>();
        gameManager = FindObjectOfType<GameManager>();

    }
    public void ClearCards()
    {
        state = CardDisplayState.Uninitialized;
        foreach (GameObject card in cards)
        {
            Destroy(card);
        }
        cards.Clear();
    }

    public void AddCard(GameObject card)
    {
        RectTransform rectTransform = card.transform as RectTransform;
        RectTransform child = rectTransform.GetChild(0).transform as RectTransform;
        rectTransform.anchoredPosition = new Vector2(0, -CardYOffset);
        child.anchoredPosition = new Vector2(0, CardYOffset);
        float y = CardYOffsetRandomRange * UnityEngine.Random.Range(0f, 1f);
        rectTransform.localPosition -= new Vector3(0f, y, 0f);
        rectTransform.rotation = Quaternion.Euler(0f, 0f, 45f);
        cards.Add(card);
        state = CardDisplayState.Initialized;
    }

    public void RemoveEventCallback(CardDisplayStateEventListener listeners)
    {
        notifyShowListeners = (CardDisplayStateEventListener)Delegate.Remove(notifyShowListeners, listeners);
        notifyHideListeners = (CardDisplayStateEventListener)Delegate.Remove(notifyHideListeners, listeners);
    }

    public void ShowAllCards(CardDisplayStateEventListener showCardsFinishedListeners = null)
    {
        if (state < CardDisplayState.Initialized)
        {
            Debug.Log("Cannot show cards - no cards added");
            showCardsFinishedListeners();
            return;
        }
        if (state >= CardDisplayState.Active && showCardsFinishedListeners != null)
        {
            showCardsFinishedListeners();
            return;
        }
        notifyShowListeners = (CardDisplayStateEventListener)Delegate.Combine(notifyShowListeners, showCardsFinishedListeners);
        if (state < CardDisplayState.ShowAnimation)
        {
            state = CardDisplayState.ShowAnimation;
            StartCoroutine(ShowAllCards());
        }
    }

    public void HideCards(CardDisplayStateEventListener hideCardsFinishedListeners = null)
    {
        if (state < CardDisplayState.Active)
        {
            Debug.Log("Can't hide card menu - cards are not yet fully displayed");
            if (hideCardsFinishedListeners != null)
            {
                hideCardsFinishedListeners();
            }
            return;
        }
        if (state >= CardDisplayState.HideAnimationFinished)
        {
            if (hideCardsFinishedListeners != null)
            {
                hideCardsFinishedListeners();
            }
            return;
        }
        notifyHideListeners = (CardDisplayStateEventListener)Delegate.Combine(notifyHideListeners, hideCardsFinishedListeners);
        if (state < CardDisplayState.HideAnimation)
        {
            state = CardDisplayState.HideAnimation;
            StartCoroutine(HideCards());
        }
    }

    private void Update()
    {
        switch (state)
        {
            case CardDisplayState.ShowAnimationFinished:
                if (notifyShowListeners != null)
                {
                    notifyShowListeners();
                }
                state = CardDisplayState.Active;
                break;
            case CardDisplayState.HideAnimationFinished:
                if (notifyHideListeners != null)
                {
                    notifyHideListeners();
                }
                state = CardDisplayState.Initialized;
                break;
            case CardDisplayState.Active:
            case CardDisplayState.HideAnimation:
                break;
        }
    }

    protected virtual IEnumerator ShowAllCards()
    {
        for (int cardIndex = 0; cardIndex < cards.Count; cardIndex++)
        {
            RotateCardForShow(cardIndex);
            yield return new WaitForSeconds(ShowOneCardAnimationLength);
        }
        state = CardDisplayState.ShowAnimationFinished;
    }

    private IEnumerator HideCards()
    {
        foreach (GameObject card in cards)
        {
            iTween.RotateTo(card.gameObject, iTween.Hash("time", HideAllCardsAnimationLength, "rotation", new Vector3(0f, 0f, -45f), "easetype", iTween.EaseType.easeOutQuint, "ignoretimescale", true));
        }
        audioManager.PlaySoundOnceAmong(SoundConfig.LevelResultsCardSlideSounds, SoundConfig.LevelResultsCardSlideVolume);
        yield return new WaitForSeconds(HideAllCardsAnimationLength);
        state = CardDisplayState.HideAnimationFinished;
    }

    protected void RotateCardForShow(int cardIndex)
    {
        cards[cardIndex].transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 45f));
        float z = GetAngleFromCardIndex(cardIndex);
        iTween.RotateTo(cards[cardIndex], iTween.Hash("time", ShowOneCardAnimationLength, "rotation", new Vector3(0f, 0f, z), "easetype", iTween.EaseType.easeOutQuint, "ignoretimescale", true));
        audioManager.PlaySoundOnceAmong(SoundConfig.LevelResultsCardSlideSounds, SoundConfig.LevelResultsCardSlideVolume);
    }

    private float GetAngleFromCardIndex(int cardIndex)
    {
        if (cards.Count == 0)
        {
            return 0f;
        }
        return CardSpacingAngle * (cards.Count - 1) - (cardIndex * 2) * CardSpacingAngle;
    }
}
