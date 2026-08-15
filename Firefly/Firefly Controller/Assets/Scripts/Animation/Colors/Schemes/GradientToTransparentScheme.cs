using UnityEngine;

namespace Firefly
{
    public class GradientToTransparentScheme : AColorScheme
    {
        public GradientToTransparentScheme() : base(new RandomSaturatedPalette()) { }
        public GradientToTransparentScheme(AColorPalette palette) : base(palette) { }

        public override AColorPattern NextColor()
        {
            Vector4 color = palette.RandomColor();
            Vector4 color2 = color;
            color2.w = 0.0f;
            return new TwoColorGradientPattern(color, color2);
        }
    }
}
