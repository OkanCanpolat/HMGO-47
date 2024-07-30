using System.Collections.Generic;
using UnityEngine;

public class SoundWave : MonoBehaviour
{
    public float GrowDuration = 0.8f;

    public float FadeOutDelay = 0.4f;

    public float NotifyAisDelay = 0.6f;

    private Projector projector;

    private Material material;

    private Node centerNode;

    private Barrier barrier;

    public float MaxSize = 5f;

    private bool notifyAisRequest;

    private float notifyAisStartTime;

    private GameManager gameManager;
    private NodeManager nodeManager;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        nodeManager = FindObjectOfType<NodeManager>();
    }
    public void Start()
    {
        projector = GetComponentInChildren<Projector>();
        projector.orthographicSize = 0f;
        projector.material = new Material(projector.material);
        material = projector.material;
        material.SetColor("_Tint", Color.black);
    }
    public void BeginAnimation(Node centerNode, Barrier barrier)
    {
        this.centerNode = centerNode;
        this.barrier = barrier;
        this.barrier.Add(this);

        transform.position = this.centerNode.transform.position;

        notifyAisStartTime = Time.time + Mathf.Min(NotifyAisDelay, GrowDuration);
        notifyAisRequest = true;

        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", MaxSize, "time", GrowDuration, "easetype", iTween.EaseType.easeOutExpo, "onupdate", "OnSizeUpdate"));
        iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "delay", FadeOutDelay, "time", GrowDuration - FadeOutDelay, "easetype", iTween.EaseType.linear, "onupdate", "OnAlphaUpdate", "oncomplete", "DestroyNoiseWave"));
    }

    private void Update()
    {
        if (notifyAisRequest && Time.time > notifyAisStartTime)
        {
            NotifyAis();
            notifyAisRequest = false;
        }
    }
    private void OnSizeUpdate(float value)
    {
        projector.orthographicSize = value;
    }

    private void OnAlphaUpdate(float value)
    {
        material.SetColor("_Tint", new Color(value, value, value));
    }
    private void NotifyAis()
    {
        float cubeHalfSize = 1.1f * nodeManager.Distance;

        int num = 0;

        foreach (Node node in nodeManager.Nodes)
        {
            Vector3 centerNodePos = centerNode.transform.position;
            centerNodePos.y = 0f;

            Vector3 nodePos = node.transform.position;
            nodePos.y = 0f;

            if (!node || !node.IsInCubeBoxDistanceFrom(centerNode, cubeHalfSize))
            {
                continue;
            }

            List<AiPawn> aiPawnsOnNode = gameManager.GetAiPawnsOnNode(node);

            foreach (AiPawn item in aiPawnsOnNode)
            {
                item.OnNoise(centerNode, barrier);
            }

            num += aiPawnsOnNode.Count;

            if (aiPawnsOnNode.Count != 0)
            {
            }
        }
    }

    private void DestroyNoiseWave()
    {
        barrier.Remove(this);
        Destroy(gameObject);
    }
}
