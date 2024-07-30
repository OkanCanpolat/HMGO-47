using UnityEngine;

public class PawnShadowOrientation : MonoBehaviour
{
    private const float groundOffset = 0.04f;

    private Vector3 m_LightForward = Vector3.zero;

    private void Start()
    {
        GameObject gameObject = GameObject.Find("Directional light");
        m_LightForward = -gameObject.transform.forward;
        m_LightForward.y = 0f;
        m_LightForward.Normalize();
        transform.localPosition += new Vector3(0f, 0f, groundOffset);
    }

    private void LateUpdate()
    {
        transform.forward = m_LightForward;
    }
}
