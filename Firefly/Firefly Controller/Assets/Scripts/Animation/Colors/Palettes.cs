using System;
using UnityEngine;

namespace Firefly
{
    /// <summary>Port of RandomSaturatedPalette.h / .cpp.</summary>
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

    /// <summary>Port of RandomDesaturatedPalette.h / .cpp.</summary>
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

    /// <summary>Port of SingleRandomHuePalette.h / .cpp.</summary>
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

    /// <summary>Port of TwoRandomHuesPalette.h / .cpp.</summary>
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

    /// <summary>Port of NarrowHueRangePalette.h / .cpp.</summary>
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

    /// <summary>Port of WideHueRangePalette.h / .cpp.</summary>
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
