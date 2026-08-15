using System;
using UnityEngine;

namespace Firefly
{
    public class TwoRandomHuesPalette : AColorPalette
    {
        public const double TRH_MIN_HUE_DISTANCE = 36.0;
        public const double TRH_MIN_SATURATION = 0.5;
        public const double TRH_MAX_SATURATION = 1.0;

        protected float hue1, hue2;

        public TwoRandomHuesPalette()
        {
            RandomizeHues();
        }

        public void RandomizeHues()
        {
            hue1 = (float)FireflyUtils.Rand(0.0, 360.0);
            do
            {
                hue2 = (float)FireflyUtils.Rand(0.0, 360.0);
            } while (Math.Abs(hue1 - hue2) < TRH_MIN_HUE_DISTANCE);
        }

        public override Vector4 NextColor()
        {
            Vector4 color = new Vector4();
            float hue = FireflyUtils.Rand1() > 0.5 ? hue1 : hue2;
            float sat = (float)FireflyUtils.Rand(TRH_MIN_SATURATION, TRH_MAX_SATURATION);
            float val = 1.0f;
            HSVRGB.HSVtoRGB(out color.x, out color.y, out color.z, hue, sat, val);
            color.w = 1.0f;

            CheckAgainstLastColor(color);
            return color;
        }
    }
}
