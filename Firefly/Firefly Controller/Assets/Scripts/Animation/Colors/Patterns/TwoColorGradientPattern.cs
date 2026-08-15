using UnityEngine;

namespace Firefly
{
    public class TwoColorGradientPattern : AColorPattern
    {
        public Vector4 color2;

        public TwoColorGradientPattern()
            : this(new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f)) { }

        public TwoColorGradientPattern(Vector4 color, Vector4 color2) : base(color)
        {
            this.color2 = color2;
        }

        public override Vector4 GetColor() { return color; }

        public override Vector4 GetColor(double x)
        {
            return (float)(1.0 - x) * color + (float)x * color2;
        }
    }
}
