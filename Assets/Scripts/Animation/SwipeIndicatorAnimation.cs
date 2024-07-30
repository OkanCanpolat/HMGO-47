using System;
using System.Collections.Generic;
using UnityEngine;

public class SwipeIndicatorAnimation : IndicatorAnimation
{
    [Serializable]
    public class MoveParam
    {
        public float innerOffset = 0.14f;

        public float outerOffset = 0.18f;

        public float tweenTime = 1.2f;
    }

    public float HintFadeTime = 0.3f;

    public Material HintMaterial;

    public GameObject ArrowObject;

    public MoveParam[] m_MoveParam;

    private GameManager gameManager;

    private void Awake()
    {
        iTween.Init(ArrowObject);
        gameManager = FindObjectOfType<GameManager>();
    }

    private void OnDisable()
    {
        iTween.Stop(ArrowObject);
    }

    private void OnEnable()
    {
        iTween.Init(ArrowObject);
    }

    public override void StartAnimation()
    {
        base.StartAnimation();

        for (int i = 0; i < 1; i++)
        {
            MoveParam moveParam = m_MoveParam[i];
            Vector3 closedTransformPosition = GetClosedTransformPosition(gameManager.PlayerPawn.CurrentNode);
            Vector3 position = currentNode.transform.position;
            position.y = closedTransformPosition.y;
            float num2 = 1f;
            if (gameManager.PlayerPawn.CurrentNode.Pawns.Count > 1)
            {
                num2 = 1.5f;
            }
            Vector3 position2 = Vector3.Lerp(closedTransformPosition, position, moveParam.innerOffset * num2);
            Vector3 vector = Vector3.Lerp(closedTransformPosition, position, moveParam.outerOffset * num2);
            transform.rotation = Quaternion.LookRotation((position - closedTransformPosition).normalized);
           
            ArrowObject.transform.position = position2;
            iTween.MoveTo(ArrowObject, iTween.Hash("position", vector, "time", moveParam.tweenTime, "easetype", iTween.EaseType.easeOutCubic, "looptype", iTween.LoopType.loop));
        }
        UpdateIndicatorMaterial();
    }

    private Vector3 GetHighestPosition(Node innerNode, Node outerNode)
    {
        Connection connectionData = innerNode.GetConnectionData(outerNode);
        Vector3 result = ((!(innerNode.transform.position.y > outerNode.transform.position.y)) ? outerNode.transform.position : innerNode.transform.position);
        foreach (GameObject handle in connectionData.Handles)
        {
            if (result.y < handle.transform.position.y)
            {
                result = handle.transform.position;
            }
        }
        return result;
    }

    private Vector3 GetClosedTransformPosition(Node node)
    {
        float num = float.MaxValue;
        Vector3 result = node.transform.position;
        List<Transform> list = new List<Transform>();

        for (int i = 0; i < node.Pawns.Count; i++)
        {
            list.Add(node.Pawns[i].transform);
        }
        if ((bool)node.CachedNodeItem)
        {
            list.Add(node.CachedNodeItem.transform);
        }
        for (int j = 0; j < list.Count; j++)
        {
            Vector3 position = list[j].transform.position;
            float sqrMagnitude = (position - currentNode.transform.position).sqrMagnitude;
            if (sqrMagnitude < num)
            {
                num = sqrMagnitude;
                result = node.transform.position + Vector3.Project(position - node.transform.position, (node.transform.position - currentNode.transform.position).normalized);
            }
        }
        return result;
    }

    protected override void UpdateIndicatorMaterial()
    {
        base.UpdateIndicatorMaterial();
    }
}
