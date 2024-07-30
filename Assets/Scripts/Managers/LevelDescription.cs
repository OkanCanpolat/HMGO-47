using UnityEngine;

[CreateAssetMenu (fileName = "Level Description")]
public class LevelDescription : ScriptableObject
{
    public int MaxStarCount = 3;
    public int Chapter;
    public int Level;
}
