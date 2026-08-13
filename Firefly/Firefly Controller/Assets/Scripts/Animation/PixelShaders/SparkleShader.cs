using System;
using UnityEngine;

namespace Firefly
{
    /// <summary>
    /// Port of SparkleShader.h / .cpp.
    ///
    /// Two quirks carried over verbatim from the original — see Port Notes:
    ///  - RenderPixel uses the SPARKLE_* constants, not the instance fields the
    ///    constructor stores, so the registered presets' parameters have no effect.
    ///  - GetStateKey returns "sparkleState" on its first line; the per-instance
    ///    key-building code below it is unreachable.
    /// </summary>
    public class SparkleShader : APixelShader
    {
        public const int SPARKLE_RISE = 20;
        public const int SPARKLE_FALL = 30;
        public const double SPARKLE_PROPORTION = 0.90;
        public static readonly int SPARKLE_CREATE_CHANCE = (int)((SPARKLE_RISE + SPARKLE_FALL) / SPARKLE_PROPORTION);
        public const double SPARKLE_BRIGHTNESS = 1.0;
        public const double SPARKLE_MAX = 1.5;
        public const double SPARKLE_MIN = 0.0;

        private int sparklesToCreate = 0;
        private int sparkleRise, sparkleFall, sparkleCreateChance;
        private double sparkleProportion, sparkleMax, sparkleMin;

        public SparkleShader(
            int sparkleRise = SPARKLE_RISE,
            int sparkleFall = SPARKLE_FALL,
            double sparkleProportion = SPARKLE_PROPORTION,
            double sparkleMax = SPARKLE_MAX,
            double sparkleMin = SPARKLE_MIN)
        {
            this.sparkleRise = sparkleRise;
            this.sparkleFall = sparkleFall;
            this.sparkleProportion = sparkleProportion;
            this.sparkleMax = sparkleMax;
            this.sparkleMin = sparkleMin;
            this.sparkleCreateChance = (int)((sparkleRise + sparkleFall) / sparkleProportion);
        }

        public override Vector4 RenderPixel(Vector3 pos, Vector4 color, ArbitraryMap details)
        {
            if (FireflyUtils.Rand() % SPARKLE_CREATE_CHANCE == 0)
                sparklesToCreate++;

            string key = GetStateKey();

            if (!details.HasKey(key))
            {
                details.SetInt(key, 0);
            }

            if (details.GetInt(key) == 0 && sparklesToCreate > 0)
            {
                details.SetInt(key, 1);
                sparklesToCreate--;
            }

            int ss = details.GetInt(key);

            double intensity;
            if (ss <= SPARKLE_RISE)
            {
                intensity = (double)ss / SPARKLE_RISE;
            }
            else
            {
                ss -= SPARKLE_RISE;
                intensity = 1.0 - (double)ss / SPARKLE_FALL;
            }

            intensity = intensity * (SPARKLE_MAX - SPARKLE_MIN) + SPARKLE_MIN;
            ApplyIntensity(ref color, intensity);

            if (ss > 0)
            {
                details.SetInt(key, ss + 1);
            }
            if (ss == SPARKLE_RISE + SPARKLE_FALL)
            {
                details.SetInt(key, 0);
            }

            return color;
        }

        private void ApplyIntensity(ref Vector4 color, double intensity)
        {
            double maxColor;
            if (color.x > color.y && color.x > color.z) maxColor = color.x;
            else if (color.y > color.z && color.y > color.x) maxColor = color.y;
            else maxColor = color.z;

            double maxMultiplier = 1.0 / maxColor;
            double multiplier = Math.Max(0.0, Math.Min(maxMultiplier, (intensity * SPARKLE_BRIGHTNESS) + 1.0));

            //HACK
            multiplier = intensity;
            color.x *= (float)multiplier;
            color.y *= (float)multiplier;
            color.z *= (float)multiplier;
        }

        private string GetStateKey()
        {
            return "sparkleState";
        }
    }
}
