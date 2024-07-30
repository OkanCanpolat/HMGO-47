
public class LimitedTurnsObjective : Objective
{
    public int MaxTurnsCount = 10;

    public override void Awake()
    {
        IsMainObjective = false;
        Description = "TURNS OR FEWER";
        base.Awake();
    }
    public override bool IsComplete()
    {
        if (gameManager.TurnsCount <= MaxTurnsCount)
        {
            return true;
        }
        return false;
    }

    public override string GetDesc()
    {
        string text = MaxTurnsCount + " " + Description;
        return " " + text.ToUpper() + " ";
    }
}
