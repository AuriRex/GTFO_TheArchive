namespace TheArchive.Core.Models
{
    public struct SColor
    {
        public static readonly SColor WHITE = new SColor();

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
    }
}
