namespace TheArchive.Core.Models;

public struct SColor
{
    public static readonly SColor WHITE = new SColor(1f, 1f, 1f);
    public static readonly SColor BLACK = new SColor(0f, 0f, 0f);

    public static readonly SColor ORANGE = new SColor(1f, 0.5f, 0.05f, 1f);
    public static readonly SColor RED = new SColor(0.8f, 0.1f, 0.1f, 1f);
    public static readonly SColor GREEN = new SColor(0.1f, 0.8f, 0.1f, 1f);

    public static readonly SColor DARK_PURPLE = new SColor(0.3f, 0.03f, 0.6f, 1f);
    public static readonly SColor DARK_ORANGE = new SColor(0.8f, 0.3f, 0.03f, 1f);

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

    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }

    public SColor WithAlpha(float alpha)
    {
        return new SColor
        {
            A = alpha,
            R = this.R,
            G = this.G,
            B = this.B,
        };
    }
}