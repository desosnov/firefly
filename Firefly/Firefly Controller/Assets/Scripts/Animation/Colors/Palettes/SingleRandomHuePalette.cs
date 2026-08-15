using UnityEngine;

namespace Firefly
{
    public class SingleRandomHuePalette : AColorPalette
    {
        public const double SRH_MIN_SATURATION = 0.5;
        public const double SRH_MAX_SATURATION = 1.0;

        protected float hue;

        public SingleRandomHuePalette()
        {
            RandomizeHue();
        }

        public void RandomizeHue()
        {
            hue = (float)FireflyUtils.Rand(0.0, 360.0);
        }

        public override Vector4 NextColor()
        {
            Vector4 color = new Vector4();
            float sat = (float)FireflyUtils.Rand(SRH_MIN_SATURATION, SRH_MAX_SATURATION);
            float val = 1.0f;
            HSVRGB.HSVtoRGB(out color.x, out color.y, out color.z, hue, sat, val);
            color.w = 1.0f;

            CheckAgainstLastColor(color);
            return color;
        }
    }
}
