using UnityEngine;

/// <summary>
/// Details regarding a texture in a texture atlas. Used for optimization.
/// </summary>
public struct TextureAtlasDetails
{
    public Texture2D Texture;
    public Vector2 TextureOffset;
    public Vector2 TextureScale;

    public TextureAtlasDetails(Texture2D texture, Vector2 offset, Vector2 scale)
    {
        Texture = texture;
        TextureOffset = offset;
        TextureScale = scale;
    }
}
