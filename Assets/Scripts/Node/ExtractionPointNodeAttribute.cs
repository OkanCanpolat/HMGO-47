using UnityEngine;

public class ExtractionPointNodeAttribute : NodeAttribute
{
    private void Awake()
    {
        Transform parent = transform.parent;

        if ((bool)parent)
        {
            GameObject gameObject = parent.gameObject;
            Node component = gameObject.GetComponent<Node>();

            if ((bool)component)
            {
                ExtractionPointObjective a_Objective = gameObject.AddComponent<ExtractionPointObjective>();
                gameManager.RegisterStarObjective(a_Objective);
            }
        }
    }
    protected override void Start()
    {
        base.Start();
    }
}
