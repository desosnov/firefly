using UnityEngine;

namespace Firefly
{
    public class GradientToBlackScheme : AColorScheme
    {
        public GradientToBlackScheme() : base(new RandomSaturatedPalette()) { }
        public GradientToBlackScheme(AColorPalette palette) : base(palette) { }

        public override AColorPattern NextColor()
        {
            return new TwoColorGradientPattern(palette.RandomColor(), new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
        }
    }
}
