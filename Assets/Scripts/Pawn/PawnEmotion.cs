using UnityEngine;
public enum PawnEmotionType
{
    Exclamation,
    Question,
    DontKill,
    ReturnToPatrol,
    CanSee,
    CantSee,
    None
}
public class PawnEmotion : MonoBehaviour
{
    public PawnEmotionType EmotionType;
}
