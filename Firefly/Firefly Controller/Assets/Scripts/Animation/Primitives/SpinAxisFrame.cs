using UnityEngine;

namespace Firefly
{
    /// <summary>
    /// A moving, rotating coordinate frame: a centre point, an up axis to spin around,
    /// and a pointing direction marking the zero-angle reference. Not an APrimitive —
    /// it doesn't render a colour itself, it reduces a world position down to three
    /// numbers (angle around the axis, distance from the axis, height along the axis)
    /// for a caller to make its own colour/alpha decisions from. SpinnerAnimation's
    /// Slice is the first consumer.
    ///
    /// upAxis and pointingAxis don't need to be unit length or mutually perpendicular —
    /// RebuildBasis() normalizes upAxis and re-derives pointingAxis as the component of
    /// pointingAxis perpendicular to it (Gram-Schmidt), same as the constructor's own
    /// "always normalized to a right angle from the up axis" rule. Set the public fields,
    /// then call RebuildBasis() once per frame before calling Transform() — the basis is
    /// cached rather than recomputed per pixel.
    ///
    /// Only Vector3 is used here, no Quaternion — see Firefly Manual §4.12: the engine
    /// holds no Unity references beyond Vector3/Vector4, to keep animation code portable.
    /// </summary>
    public class SpinAxisFrame
    {
        public Vector3 centerPos;
        public Vector3 upAxis;
        public Vector3 pointingAxis;

        private Vector3 basisUp, basisPointing, basisRight;

        public SpinAxisFrame(Vector3 centerPos, Vector3 upAxis, Vector3 pointingAxis)
        {
            this.centerPos = centerPos;
            this.upAxis = upAxis;
            this.pointingAxis = pointingAxis;
            RebuildBasis();
        }

        public void RebuildBasis()
        {
            basisUp = upAxis.normalized;

            Vector3 pointing = pointingAxis - basisUp * Vector3.Dot(pointingAxis, basisUp);
            if (pointing.sqrMagnitude < 1e-8f)
            {
                // Degenerate: pointingAxis was parallel to upAxis. Fall back to any
                // vector not parallel to up, then strip the up-component from that.
                Vector3 fallback = Mathf.Abs(basisUp.y) < 0.99f ? Vector3.up : Vector3.right;
                pointing = fallback - basisUp * Vector3.Dot(fallback, basisUp);
            }
            basisPointing = pointing.normalized;

            // Completes the frame. Which way "right" ends up facing in world space is
            // arbitrary — it only has to be consistent frame to frame, which it is.
            basisRight = Vector3.Cross(basisUp, basisPointing).normalized;
        }

        /// <summary>
        /// angle: radians around the up axis, 0 = pointing direction, increasing toward
        /// basisRight. distance: perpendicular distance from the up-axis line. height:
        /// signed distance along the up axis, 0 at centerPos.
        /// </summary>
        public void Transform(Vector3 worldPos, out double angle, out double distance, out double height)
        {
            Vector3 rel = worldPos - centerPos;
            double x = Vector3.Dot(rel, basisRight);
            double z = Vector3.Dot(rel, basisPointing);
            double y = Vector3.Dot(rel, basisUp);

            angle = System.Math.Atan2(x, z);
            distance = System.Math.Sqrt(x * x + z * z);
            height = y;
        }
    }
}
