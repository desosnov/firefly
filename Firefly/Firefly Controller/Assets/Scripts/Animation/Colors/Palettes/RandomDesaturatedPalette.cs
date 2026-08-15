using UnityEngine;

namespace Firefly
{
    public class RandomDesaturatedPalette : AColorPalette
    {
        public const double RDP_DEFAULT_MAX_SATURATION = 0.75;
        public const double RDP_MIN_SATURATION = 0.5;
        public const double RDP_MAX_SATURATION = 0.9;

        private double maxSaturation;

        public RandomDesaturatedPalette(double maxSaturation = RDP_DEFAULT_MAX_SATURATION)
        {
            this.maxSaturation = maxSaturation;
        }

        public void SetMaxSaturation(double sat) { maxSaturation = sat; }

        public override Vector4 NextColor()
        {
            Vector4 newColor = new Vector4();
            float hue = (float)FireflyUtils.Rand(0.0, 360.0);
            float sat = (float)FireflyUtils.Rand(RDP_MIN_SATURATION, maxSaturation);
            float val = 1.0f;
            HSVRGB.HSVtoRGB(out newColor.x, out newColor.y, out newColor.z, hue, sat, val);
            newColor.w = 1.0f;

            CheckAgainstLastColor(newColor);
            return newColor;
        }
    }
}
