namespace TheArchive.Core.Models;

/// <summary>
/// Serializable color struct.
/// </summary>
public struct SColor
{
    /// <summary> Full white color. </summary>
    public static readonly SColor WHITE = new SColor(1f, 1f, 1f);
    /// <summary> Full black color. </summary>
    public static readonly SColor BLACK = new SColor(0f, 0f, 0f);

    /// <summary> A shade of orange. </summary>
    public static readonly SColor ORANGE = new SColor(1f, 0.5f, 0.05f, 1f);
    /// <summary> A shade of red. </summary>
    public static readonly SColor RED = new SColor(0.8f, 0.1f, 0.1f, 1f);
    /// <summary> A shade of green. </summary>
    public static readonly SColor GREEN = new SColor(0.1f, 0.8f, 0.1f, 1f);

    /// <summary> A darker shade of purple. </summary>
    public static readonly SColor DARK_PURPLE = new SColor(0.3f, 0.03f, 0.6f, 1f);
    /// <summary> A darker shade of orange. </summary>
    public static readonly SColor DARK_ORANGE = new SColor(0.8f, 0.3f, 0.03f, 1f);

    /// <summary>
    /// SColor constructor.
    /// </summary>
    /// <param name="r">Red color component.</param>
    /// <param name="g">Green color component.</param>
    /// <param name="b">Blue color component.</param>
    /// <param name="a">Alpha component.</param>
    public SColor(float r, float g, float b, float? a = null)
    {
        R = r;
        G = g;
        B = b;

        if(a.HasValue)
        {
            A = a.Value;
        }
        else
        {
            A = 1f;
        }
    }

    /// <summary> Red color component. </summary>
    public float R { get; set; }
    
    /// <summary> Green color component. </summary>
    public float G { get; set; }
    
    /// <summary> Blue color component. </summary>
    public float B { get; set; }
    
    /// <summary> Alpha component. </summary>
    public float A { get; set; }

    /// <summary>
    /// Copy the current color but with a different alpha value.
    /// </summary>
    /// <param name="alpha">The new alpha value.</param>
    /// <returns>The copied color.</returns>
    public readonly SColor WithAlpha(float alpha)
    {
        return this with { A = alpha };
    }
}