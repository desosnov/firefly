using UnityEngine;

namespace Firefly
{
    public class RandomSaturatedPalette : AColorPalette
    {
        public const double RSP_DEFAULT_SATURATION = 0.75;
        public const double RSP_MIN_SATURATION = 0.0;
        public const double RSP_MAX_SATURATION = 0.9;

        private double saturation;

        public RandomSaturatedPalette(double minSaturation = RSP_DEFAULT_SATURATION)
        {
            saturation = minSaturation;
        }

        public void SetMinSaturation(double sat) { saturation = sat; }

        public override Vector4 NextColor()
        {
            Vector4 newColor = new Vector4();
            float hue = (float)FireflyUtils.Rand(0.0, 360.0);
            float sat = (float)FireflyUtils.Rand(saturation, 1.0);
            float val = 1.0f;
            HSVRGB.HSVtoRGB(out newColor.x, out newColor.y, out newColor.z, hue, sat, val);
            newColor.w = 1.0f;

            CheckAgainstLastColor(newColor);
            return newColor;
        }
    }
}
