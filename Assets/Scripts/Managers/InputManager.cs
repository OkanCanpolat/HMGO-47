using UnityEngine;
using Zenject;

public class InputManager : MonoBehaviour
{
    private bool selectionActivated;
    private Vector2 clickCurrentPosition = Vector2.zero;
    private bool isActive = true;
    private bool isPlayerAtGestureStartPoint;
    private Vector2 initialPosition = Vector2.zero;
    private Vector2 previousPosition = Vector2.zero;
    private Node nodeAtClickStartPoint;
    private bool isSwipeActive;

    [Inject] private GameManager gameManager;
    [Inject] private NodeManager nodeManager;
    private CameraSphericalMovement sphericalMovement;

    public bool IsPlayerAtStartPoint => isPlayerAtGestureStartPoint;
    public bool IsSelectionActivated => selectionActivated;

    private void Awake()
    {
        sphericalMovement = Camera.main.GetComponent<CameraSphericalMovement>();
    }
    public void Activate()
    {
        isActive = true;
    }

    public void Deactivate()
    {
        isActive = false;
    }

    private void Update()
    {
        if (!isActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            clickCurrentPosition = Input.mousePosition;
            OnClick(clickCurrentPosition);
        }

        if (Input.GetMouseButton(0))
        {
            clickCurrentPosition = Input.mousePosition;
            OnDrag(clickCurrentPosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            clickCurrentPosition = Input.mousePosition;
            OnClickEnd(clickCurrentPosition);
        }
    }
    private void OnClick(Vector2 position)
    {
        if (!isActive) return;

        initialPosition = position;
        previousPosition = position;
        isPlayerAtGestureStartPoint = false;
        nodeAtClickStartPoint = null;

        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(position));

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit raycastHit = hits[i];

            if (raycastHit.collider == null)
            {
                continue;
            }

            GameObject gameObject = raycastHit.collider.gameObject;
            if (gameObject == null)
            {
                continue;
            }

            if (gameObject.transform.root != null && gameObject.transform.root.GetComponent<PlayerPawn>() != null)
            {
                isPlayerAtGestureStartPoint = true;
                continue;
            }

            Node node = gameObject.GetComponent<Node>();

            if (node != null)
            {
                nodeAtClickStartPoint = node;
            }
        }

        isSwipeActive = false;
    }
    private void OnDrag(Vector2 position)
    {
        if (!isActive) return;

        if (!isSwipeActive && Vector2.Distance(position, initialPosition) > 50f)
        {
            isSwipeActive = true;
        }
        if (isSwipeActive && !isPlayerAtGestureStartPoint)
        {
            previousPosition = position;
            Vector2 a_OffsetDelta = position - previousPosition;
            sphericalMovement.AddOffsetDelta(a_OffsetDelta);
        }
    }
    private void OnClickEnd(Vector2 position)
    {
        if (!isActive) return;

        PlayerPawn playerPawn = gameManager.PlayerPawn;

        if (isSwipeActive && !isPlayerAtGestureStartPoint)
        {
            sphericalMovement.ResetOffset();
            return;
        }

        if (!isSwipeActive && nodeAtClickStartPoint != null)
        {
            if (nodeAtClickStartPoint.IsClickable)
            {
                gameManager.OnNodeClicked(nodeAtClickStartPoint);
            }
            isPlayerAtGestureStartPoint = false;
            return;
        }

        Node node = null;
        node = ((!gameManager.IsClickAllowed) ? playerPawn.TargetNode : playerPawn.CurrentNode);

        Transform nodeTransform = node.transform;

        Vector2 to = position - initialPosition;

        if (to.magnitude > 50f)
        {
            Vector2 vector = Camera.main.WorldToScreenPoint(nodeTransform.position);
            float minAngle = float.MaxValue;
            Node targetNode = null;

            if (gameManager.IsClickAllowed)
            {
                foreach (Node sourceNode in nodeManager.Nodes)
                {
                    if (sourceNode.IsSwipable)
                    {
                        Vector2 nodePos = Camera.main.WorldToScreenPoint(sourceNode.transform.position);
                        Vector2 from = nodePos - vector;

                        float angle = Vector2.Angle(from, to);

                        if (angle < minAngle)
                        {
                            minAngle = angle;
                            targetNode = sourceNode;
                        }
                    }
                }
            }
            else
            {
                foreach (Node node4 in nodeManager.Nodes)
                {
                    Transform node2Transform = node4.transform;

                    float distance = Vector3.Distance(new Vector3(node2Transform.position.x, 0f, node2Transform.position.z), new Vector3(nodeTransform.position.x, 0f, nodeTransform.position.z));

                    if (node4 != node && distance < 4.1f)
                    {
                        Vector2 vector3 = Camera.main.WorldToScreenPoint(node2Transform.position);
                        Vector2 from2 = vector3 - vector;
                        float num4 = Vector2.Angle(from2, to);
                        if (num4 < minAngle)
                        {
                            minAngle = num4;
                            targetNode = node4;
                        }
                    }
                }
            }
            if (targetNode != null && minAngle < 45f)
            {
                gameManager.OnNodeClicked(targetNode);
            }
        }

        isPlayerAtGestureStartPoint = false;
    }


}
