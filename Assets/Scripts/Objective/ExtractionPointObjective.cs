public class ExtractionPointObjective : Objective
{
    public override void Awake()
    {

        IsMainObjective = true;

        if (Description == string.Empty)
        {
            Description = "LEVEL COMPLETE";
        }
        base.Awake();

    }

    public override bool IsComplete()
    {
        Node component = gameObject.GetComponent<Node>();

        if (gameManager.PlayerPawn.CurrentNode == component)
        {
            return true;
        }

        return false;
    }
}
