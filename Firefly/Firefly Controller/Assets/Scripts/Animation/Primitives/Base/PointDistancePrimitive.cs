using UnityEngine;

namespace Firefly
{
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
}
