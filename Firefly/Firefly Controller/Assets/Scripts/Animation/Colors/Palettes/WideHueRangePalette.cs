using UnityEngine;

namespace Firefly
{
    public class WideHueRangePalette : AColorPalette
    {
        public const double WHR_MIN_SATURATION = 0.5;
        public const double WHR_MAX_SATURATION = 1.0;
        public const double WHR_MIN_HUE_RANGE = 100.0;
        public const double WHR_MAX_HUE_RANGE = 200.0;

        protected float minHue, maxHue;

        public WideHueRangePalette()
        {
            RandomizeHue();
        }

        public void RandomizeHue()
        {
            minHue = (float)FireflyUtils.Rand(0.0, 360.0);
            maxHue = minHue + (float)FireflyUtils.Rand(WHR_MIN_HUE_RANGE, WHR_MAX_HUE_RANGE);
        }

        public override Vector4 NextColor()
        {
            Vector4 color = new Vector4();
            float hue = (float)FireflyUtils.Rand(minHue, maxHue) % 360.0f;
            float sat = (float)FireflyUtils.Rand(WHR_MIN_SATURATION, WHR_MAX_SATURATION);
            float val = 1.0f;
            HSVRGB.HSVtoRGB(out color.x, out color.y, out color.z, hue, sat, val);
            color.w = 1.0f;

            CheckAgainstLastColor(color);
            return color;
        }
    }
}
