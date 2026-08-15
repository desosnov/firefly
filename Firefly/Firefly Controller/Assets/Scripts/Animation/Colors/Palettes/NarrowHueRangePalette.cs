using UnityEngine;

namespace Firefly
{
    public class NarrowHueRangePalette : AColorPalette
    {
        public const double NHR_MIN_SATURATION = 0.5;
        public const double NHR_MAX_SATURATION = 1.0;
        public const double NHR_MIN_HUE_RANGE = 20.0;
        public const double NHR_MAX_HUE_RANGE = 100.0;

        protected float minHue, maxHue;

        public NarrowHueRangePalette()
        {
            RandomizeHue();
        }

        public void RandomizeHue()
        {
            minHue = (float)FireflyUtils.Rand(0.0, 360.0);
            maxHue = minHue + (float)FireflyUtils.Rand(NHR_MIN_HUE_RANGE, NHR_MAX_HUE_RANGE);
        }

        public override Vector4 NextColor()
        {
            Vector4 color = new Vector4();
            float hue = (float)FireflyUtils.Rand(minHue, maxHue) % 360.0f;
            float sat = (float)FireflyUtils.Rand(NHR_MIN_SATURATION, NHR_MAX_SATURATION);
            float val = 1.0f;
            HSVRGB.HSVtoRGB(out color.x, out color.y, out color.z, hue, sat, val);
            color.w = 1.0f;

            CheckAgainstLastColor(color);
            return color;
        }
    }
}
