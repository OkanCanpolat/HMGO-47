using UnityEngine;

public class ClickIndicatorAnimation : IndicatorAnimation
{
    public float ActionIndicatorInnerOffset = 0.28f;

    public float ActionIndicatorOuterOffset = 0.35f;

    public float ActionIndicatorTweenTime = 1.2f;

    public GameObject CircleObject;

    private void Awake()
    {
        iTween.Init(CircleObject);
    }

    public override void StartAnimation()
    {
        base.StartAnimation();
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = 4f * ActionIndicatorOuterOffset * Vector3.one;
        CircleObject.transform.localScale = new Vector3(1f, 1f, 1f);
        iTween.ScaleFrom(CircleObject, iTween.Hash("scale", 4f * ActionIndicatorInnerOffset * Vector3.one, "time", ActionIndicatorTweenTime, "easetype", iTween.EaseType.easeOutExpo, "looptype", iTween.LoopType.loop));
       
        UpdateIndicatorMaterial();
    }

    protected override void UpdateIndicatorMaterial()
    {
        base.UpdateIndicatorMaterial();
    }
}
