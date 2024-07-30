using UnityEngine;

public class Objective : MonoBehaviour
{
    protected bool isMainObjective;

    public int ObjectiveIndex;

    public string Description = string.Empty;

    protected GameManager gameManager;

    public bool IsMainObjective { get => isMainObjective; set => isMainObjective = value; }

    public virtual void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (IsMainObjective)
        {
            gameManager.RegisterObjective(this);
        }
        else
        {
            gameManager.RegisterStarObjective(this);
        }
    }

    public virtual bool IsComplete()
    {
        return false;
    }

    public virtual string GetDesc()
    {
        return " " + Description + " ";
    }
}
