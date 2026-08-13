using System;
using System.Collections.Generic;
using UnityEngine;

namespace Firefly
{
    /// <summary>Port of PointDistancePrimitive.h / .cpp.</summary>
    public abstract class PointDistancePrimitive : APrimitive
    {
        public Vector3 centerPos = new Vector3(0.0f, 0.0f, 0.0f);

        protected PointDistancePrimitive(Vector3 centerPoint)
        {
            centerPos = centerPoint;
        }

        protected abstract Vector4 RenderAtDistanceFromPoint(double distance, ArbitraryMap details);

        protected override Vector4 RenderPixelAt(Vector3 pos, ArbitraryMap details)
        {
            return RenderAtDistanceFromPoint(Vector3.Distance(pos, centerPos), details);
        }
    }

    /// <summary>Port of SpherePrim.h / .cpp.</summary>
    public class SpherePrim : PointDistancePrimitive
    {
        public const double SP_SOFT_EDGE_RATIO = 0.05;

        public AColorPattern colorPattern;
        public double radius;
        public int shaderIndex = -1;

        public SpherePrim(Vector3 centerPoint, double radius, AColorPattern colorPattern, int shaderIndex = -1)
            : base(centerPoint)
        {
            this.radius = radius;
            this.colorPattern = colorPattern;
            this.shaderIndex = shaderIndex;
        }

        protected override Vector4 RenderAtDistanceFromPoint(double distance, ArbitraryMap details)
        {
            float range = (float)(distance / radius);
            if (range <= 1.0)
            {
                details.SetInt("shaderIndex", shaderIndex);
                return colorPattern.GetColor(1.0f - (distance / radius) * (distance / radius));
            }
            else if (range < 1.0 + SP_SOFT_EDGE_RATIO)
            {
                float alpha = (float)(1.0 - (distance / radius - 1.0) / SP_SOFT_EDGE_RATIO);
                Vector4 color = colorPattern.GetColor(0.0);
                color *= alpha;
                color.w = 1.0f;
                return color;
            }
            else if (range < 1.0 + SP_SOFT_EDGE_RATIO + 0.1)
            {
                float alpha = (float)(Math.Sin(1.0 - (range - 1.0 - SP_SOFT_EDGE_RATIO) / 0.1) * FireflyUtils.M_PI / 2.0);
                return new Vector4(0.0f, 0.0f, 0.0f, alpha);
            }
            else
            {
                return new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            }
        }
    }

    /// <summary>Port of ConcentricSpheresPrim.h / .cpp.</summary>
    public class ConcentricSpheresPrim : PointDistancePrimitive
    {
        public const double CSP_MIN_ALPHA = 0.85;
        public const double CSP_EDGE_FADE_SIZE = 3.0;

        public List<Vector4> colors;
        public double colorBlendingFactor;
        public double ringRadius, sizeInRings;
        public int finalRing = int.MaxValue;

        protected IntervalSlicerWithSymmetricalPhases slicer;

        public ConcentricSpheresPrim(
            Vector3 centerPoint,
            double ringRadius,
            double sizeInRings,
            List<Vector4> colors,
            double colorBlendingFactor)
            : base(centerPoint)
        {
            this.ringRadius = ringRadius;
            this.sizeInRings = sizeInRings;
            this.colors = colors;
            this.colorBlendingFactor = colorBlendingFactor;
            this.slicer = new IntervalSlicerWithSymmetricalPhases(0.0, ringRadius);
        }

        protected override Vector4 RenderAtDistanceFromPoint(double distance, ArbitraryMap details)
        {
            double distanceIntoSphere = sizeInRings * ringRadius - distance;

            slicer.interval = ringRadius;
            int ring = slicer.GetInterval(distanceIntoSphere);
            double phase = slicer.GetPhase(distanceIntoSphere);

            // FireflyUtils.Mod rather than % — the C++ indexed with a possibly-negative
            // result, which C# would throw on. See Port Notes.
            int colorIndex = FireflyUtils.Mod(ring, colors.Count);
            int blendColorIndex = phase <= 0.5
                ? FireflyUtils.Mod(ring - 1, colors.Count)
                : FireflyUtils.Mod(ring + 1, colors.Count);

            float colorBrightness = (float)(Math.Max(0.0, slicer.GetSymmetricalPhase(distanceIntoSphere) + colorBlendingFactor) / (1.0 + colorBlendingFactor));
            float blendColorBrightness = (float)(Math.Max(0.0, colorBlendingFactor - slicer.GetSymmetricalPhase(distanceIntoSphere)) / (1.0 + colorBlendingFactor));

            Vector4 color;
            if (ring < 0 || ring > finalRing)
            {
                color = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            }
            else if ((ring == 0 && phase < 0.5) || (ring == finalRing && phase > 0.5))
            {
                color = colors[colorIndex];
                color.w = (float)Math.Min(colorBrightness, (ring == 0 ? phase : 1.0 - phase) / 0.5);
            }
            else
            {
                color = colors[colorIndex] * (1.0f - blendColorBrightness) + colors[blendColorIndex] * blendColorBrightness;
                color *= colorBrightness + blendColorBrightness;
                color += new Vector4(0.0f, 0.0f, 0.0f, (float)CSP_MIN_ALPHA) * (1.0f - colorBrightness - blendColorBrightness);
            }

            float distToEdge = (float)Math.Min((double)ring + phase, (double)finalRing + 1.0 - (double)ring - phase);
            float edgeAlphaMultiplier = (float)Math.Max(0.0, Math.Min(1.0, distToEdge / CSP_EDGE_FADE_SIZE));
            color.w *= edgeAlphaMultiplier;

            if (colorBrightness > 0.0 && ring >= 0)
            {
                details.SetInt("shaderIndex", ring);
            }
            return color;
        }
    }
}
