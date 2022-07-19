namespace TheArchive.Core.Models
{
    public class SColor
    {
        public static readonly SColor WHITE = new SColor();

        public SColor() { }

        public SColor(float r, float g, float b, float? a = null)
        {
            R = r;
            G = g;
            B = b;

            if(a.HasValue)
            {
                A = a.Value;
            }
        }

        public float R { get; set; } = 1f;
        public float G { get; set; } = 1f;
        public float B { get; set; } = 1f;
        public float A { get; set; } = 1f;
    }
}
