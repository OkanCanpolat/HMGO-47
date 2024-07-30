using System.Collections.Generic;
using UnityEngine;

public class SecretPassageNodeAttribute : NodeAttribute
{
    public List<SecretPassageAnimation> EntranceObjects = new List<SecretPassageAnimation>();

    public List<SecretPassageAnimation> ExitObjects = new List<SecretPassageAnimation>();

    public Node TargetNode;

    public SecretPaasageSoundConfig SoundConfig;

    protected override void Start()
    {
        base.Start();

        if (TargetNode == null)
        {
            Debug.LogError("Secret Passage Node Attribute has no Target node.");
        }
    }

    public override void OnEnterNodeSelection(bool shouldSelectNodeForInteraction)
    {
        if (gameObject.activeInHierarchy && !shouldSelectNodeForInteraction && TargetNode != null)
        {
            TargetNode.DisplayHatchIndicator();
        }
    }
}
