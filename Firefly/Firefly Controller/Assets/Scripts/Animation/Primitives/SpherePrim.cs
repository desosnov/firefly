using System;
using UnityEngine;

namespace Firefly
{
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
}
