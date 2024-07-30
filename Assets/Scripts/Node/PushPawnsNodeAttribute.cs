using UnityEngine;

public class PushPawnsNodeAttribute : NodeAttribute
{
    public Orientation Direction;

    public Material ScrollingMaterial;

    public string ScrollingTextureName = "_MainTex";

    public Vector2 ScrollingUvPerSecond = new Vector2(1f, 0f);

    private Vector2 ScrollingUvOffset = Vector2.zero;

    private bool IsScrollingTexture;

    private void Update()
    {
        if (IsScrollingTexture)
        {
            ScrollingUvOffset += ScrollingUvPerSecond * Time.deltaTime;

            if (ScrollingMaterial != null)
            {
                ScrollingMaterial.SetTextureOffset(ScrollingTextureName, ScrollingUvOffset);
            }
        }
    }
}
