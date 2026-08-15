using System;
using System.Collections.Generic;
using UnityEngine;

namespace Firefly
{
    /// <summary>
    /// A wedge-shaped "clock hand" animation built on SpinAxisFrame. A shared frame
    /// (moving centre, tilting axis, constantly turning) is tested by a list of angular
    /// Slices, each painting the wedge of the circle it currently occupies. Slices grow
    /// in count toward a random upper bound, then shrink toward a random lower bound,
    /// repeating for the life of the animation; wind-down abandons whichever phase it's
    /// in and shrinks to zero.
    /// </summary>
    public class SpinnerAnimation : AAnimation
    {
        // SPIN = SpinnerAnimation

        // How many slices exist at once, cycling between these two bounds for the life
        // of the animation. Picked once at Init; lowerBound stays at least
        // SPIN_BOUND_MIN_GAP below whatever upperBound was drawn.
        public const int SPIN_UPPER_MIN = 5;
        public const int SPIN_UPPER_MAX = 20;
        public const int SPIN_LOWER_MIN = 2;
        public const int SPIN_LOWER_MAX = 8;
        public const int SPIN_BOUND_MIN_GAP = 3;

        // Seconds between adding or removing one slice. Picked once at Init.
        public const double SPIN_PACE_MIN = 1.0;
        public const double SPIN_PACE_MAX = 5.0;

        // Fraction of the maximum gap-free slice width each slice actually uses.
        // One shared value for the whole animation, picked once at Init.
        public const double SPIN_OCCUPANCY_MIN = 0.5;
        public const double SPIN_OCCUPANCY_MAX = 1.0;

        // Extra multiplier on every slice's width, oscillating 0.01 <-> 1.0 on a
        // cosine curve so it starts at the low end. Period picked once at Init.
        public const double SPIN_WIDTH_MULT_MIN = 0.01;
        public const double SPIN_WIDTH_MULT_MAX = 1.0;
        public const double SPIN_WIDTH_MULT_PERIOD_MIN = 5.0;
        public const double SPIN_WIDTH_MULT_PERIOD_MAX = 20.0;

        // How long each drift to a new centre/up/pointing takes. Re-picked every time
        // a drift completes, same pattern as SpheresAnimation.SetUpNextMove.
        public const double SPIN_AXIS_MOVE_MIN = 10.0;
        public const double SPIN_AXIS_MOVE_MAX = 30.0;

        // Rotation speed oscillates between these on a sine wave. Degrees here, radians
        // everywhere else — converted once at Init. Oscillation period isn't specced;
        // starting range chosen in the same spirit as the animation's other periods.
        public const double SPIN_ROT_LOW_MIN_DEG = 1.0;
        public const double SPIN_ROT_LOW_MAX_DEG = 10.0;
        public const double SPIN_ROT_HIGH_MIN_DEG = 60.0;
        public const double SPIN_ROT_HIGH_MAX_DEG = 300.0;
        public const double SPIN_ROT_OSC_PERIOD_MIN = 8.0;
        public const double SPIN_ROT_OSC_PERIOD_MAX = 20.0;

        // Hard-coded fade widths at each slice edge: full colour, then fade to black
        // over SPIN_FADE_BLACK_RADIANS, then fade to transparent over
        // SPIN_FADE_TRANSPARENT_RADIANS. Applied identically on both edges.
        public const double SPIN_FADE_BLACK_DEGREES = 3.0;
        public const double SPIN_FADE_TRANSPARENT_DEGREES = 1.0;
        public static readonly double SPIN_FADE_BLACK_RADIANS = SPIN_FADE_BLACK_DEGREES * FireflyUtils.M_PI / 180.0;
        public static readonly double SPIN_FADE_TRANSPARENT_RADIANS = SPIN_FADE_TRANSPARENT_DEGREES * FireflyUtils.M_PI / 180.0;

        // Stop compositing further-back slices once accumulated alpha crosses this.
        public const float SPIN_ALPHA_RENDER_CUTOFF = 0.95f;

        private class Slice
        {
            public double angle;
            public double width;
            public Vector4 color;
            public bool removing;

            public AEasingFunction1D angleEase;
            public AEasingFunction1D widthEase;
        }

        private SpinAxisFrame frame;
        private readonly List<Slice> slices = new List<Slice>();

        private int upperBound, lowerBound;
        private bool growing;
        private double slicePace;
        private double sliceOccupancy;
        private double nextSliceEventTime;

        private double sliceWidthMultiplier;
        private double widthMultiplierPeriod;

        private AEasingFunction3D centerEase, upEase, pointingEase;

        private double rotSpeedLow, rotSpeedHigh, rotSpeedOscPeriod;
        private double rotationOffset;
        private double lastUpdateTime;

        private bool wrappingUp = false, nextAnimationFlag = false, finishedFlag = false;

        public SpinnerAnimation(PixelStage stage)
            : this(stage, new RandomSaturatedPalette(), new SolidColorsScheme()) { }

        public SpinnerAnimation(PixelStage stage, AColorPalette palette, AColorScheme colorScheme)
            : base(stage, palette, colorScheme)
        {
            // Slice edges are hard boundaries (softened only by the fixed-width fade),
            // so this benefits from subpixel sampling the way sphere edges sometimes do.
            subpixelSampling = true;
        }

        protected override void InitInternal()
        {
            wrappingUp = false;
            nextAnimationFlag = false;
            finishedFlag = false;
            slices.Clear();

            upperBound = (int)Math.Round(FireflyUtils.Rand(SPIN_UPPER_MIN, SPIN_UPPER_MAX));
            lowerBound = (int)Math.Min(
                Math.Round(FireflyUtils.Rand(SPIN_LOWER_MIN, SPIN_LOWER_MAX)),
                upperBound - SPIN_BOUND_MIN_GAP);
            growing = true;

            slicePace = FireflyUtils.Rand(SPIN_PACE_MIN, SPIN_PACE_MAX);
            sliceOccupancy = FireflyUtils.Rand(SPIN_OCCUPANCY_MIN, SPIN_OCCUPANCY_MAX);
            nextSliceEventTime = 0.0;

            widthMultiplierPeriod = FireflyUtils.Rand(SPIN_WIDTH_MULT_PERIOD_MIN, SPIN_WIDTH_MULT_PERIOD_MAX);
            sliceWidthMultiplier = SPIN_WIDTH_MULT_MIN;

            rotSpeedLow = FireflyUtils.Rand(SPIN_ROT_LOW_MIN_DEG, SPIN_ROT_LOW_MAX_DEG) * FireflyUtils.M_PI / 180.0;
            rotSpeedHigh = FireflyUtils.Rand(SPIN_ROT_HIGH_MIN_DEG, SPIN_ROT_HIGH_MAX_DEG) * FireflyUtils.M_PI / 180.0;
            rotSpeedOscPeriod = FireflyUtils.Rand(SPIN_ROT_OSC_PERIOD_MIN, SPIN_ROT_OSC_PERIOD_MAX);
            rotationOffset = 0.0;
            lastUpdateTime = 0.0;

            frame = new SpinAxisFrame(
                stage.GetCentroid() + RandomUnitVector() * (float)stage.GetMaxRadius(),
                RandomUnitVector(),
                RandomUnitVector());
            SetUpNextAxisMove(0.0);
        }

        protected override void UpdateInternal(double time)
        {
            double dt = Math.Max(0.0, time - lastUpdateTime);
            lastUpdateTime = time;

            // Rotation speed oscillates low <-> high; the offset is its running integral.
            double rotPhase = (time / rotSpeedOscPeriod) * 2.0 * FireflyUtils.M_PI;
            double rotSpeed = rotSpeedLow + (rotSpeedHigh - rotSpeedLow) * (0.5 + 0.5 * Math.Sin(rotPhase));
            rotationOffset += rotSpeed * dt;

            // Width multiplier: cosine so it starts at the low end, not the sine
            // midpoint, matching "starting at 0" against a 0.01-1.0 range.
            double widthPhase = (time / widthMultiplierPeriod) * 2.0 * FireflyUtils.M_PI;
            sliceWidthMultiplier = SPIN_WIDTH_MULT_MIN
                + (SPIN_WIDTH_MULT_MAX - SPIN_WIDTH_MULT_MIN) * (0.5 - 0.5 * Math.Cos(widthPhase));

            if (centerEase.Finished()) SetUpNextAxisMove(time);
            centerEase.Update(time);
            upEase.Update(time);
            pointingEase.Update(time);
            frame.RebuildBasis();

            for (int i = 0; i < slices.Count; i++)
            {
                slices[i].angleEase.Update(time);
                slices[i].widthEase.Update(time);
            }

            if (!wrappingUp)
            {
                if (time >= nextSliceEventTime)
                {
                    if (growing)
                    {
                        AddSlice(time);
                        if (ActiveCount() >= upperBound) growing = false;
                    }
                    else
                    {
                        RemoveOldestSlice(time);
                        if (ActiveCount() <= lowerBound) growing = true;
                    }
                    nextSliceEventTime = time + slicePace;
                }
            }
            else if (time >= nextSliceEventTime && ActiveCount() > 0)
            {
                RemoveOldestSlice(time);
                nextSliceEventTime = time + slicePace;
            }

            slices.RemoveAll(s => s.removing && s.widthEase.Finished());

            if (wrappingUp && slices.Count == 0)
            {
                finishedFlag = true;
            }
        }

        public override Vector4 RenderPixelInternal(Vector3 pos, ArbitraryMap details)
        {
            frame.Transform(pos, out double angle, out double distance, out double height);
            double pointAngle = angle + rotationOffset;

            Vector4 result = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            for (int i = slices.Count - 1; i >= 0; i--)
            {
                Vector4 sliceColor = RenderSlice(slices[i], pointAngle);
                if (sliceColor.w == 0.0f) continue;

                result = BlendColors(sliceColor, result);
                if (result.w >= SPIN_ALPHA_RENDER_CUTOFF) break;
            }

            if (wrappingUp && result.w < 1.0f)
            {
                nextAnimationFlag = true;
            }

            return result;
        }

        public override void BeginWrappingUp() { wrappingUp = true; }
        public override bool ReadyForNextAnimation() { return nextAnimationFlag; }
        public override bool Finished() { return finishedFlag; }

        private Vector4 RenderSlice(Slice s, double pointAngle)
        {
            double halfWidth = s.width * sliceWidthMultiplier * 0.5;
            if (halfWidth <= 0.0) return new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

            double angularDist = Math.Abs(AngleDelta(s.angle, pointAngle));

            double blackZoneStart = Math.Max(0.0, halfWidth - SPIN_FADE_BLACK_RADIANS);
            double blackZoneWidth = halfWidth - blackZoneStart;
            double transparentZoneEnd = halfWidth + SPIN_FADE_TRANSPARENT_RADIANS;

            if (angularDist > transparentZoneEnd)
            {
                return new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            }

            if (angularDist <= blackZoneStart)
            {
                Vector4 full = s.color;
                full.w = 1.0f;
                return full;
            }

            if (angularDist <= halfWidth)
            {
                float brightness = blackZoneWidth > 0.0
                    ? (float)(1.0 - (angularDist - blackZoneStart) / blackZoneWidth)
                    : 0.0f;
                return new Vector4(s.color.x * brightness, s.color.y * brightness, s.color.z * brightness, 1.0f);
            }

            float alpha = (float)(1.0 - (angularDist - halfWidth) / SPIN_FADE_TRANSPARENT_RADIANS);
            return new Vector4(0.0f, 0.0f, 0.0f, Mathf.Clamp01(alpha));
        }

        private int ActiveCount()
        {
            int n = 0;
            for (int i = 0; i < slices.Count; i++)
            {
                if (!slices[i].removing) n++;
            }
            return n;
        }

        private void AddSlice(double time)
        {
            double startAngle = slices.Count > 0
                ? slices[slices.Count - 1].angle
                : FireflyUtils.Rand(0.0, 2.0 * FireflyUtils.M_PI);

            Slice s = new Slice
            {
                angle = startAngle,
                width = 0.0,
                color = palette.NextColor(),
                removing = false
            };
            slices.Add(s);

            RetargetSlices(time);
        }

        private void RemoveOldestSlice(double time)
        {
            Slice oldest = null;
            for (int i = 0; i < slices.Count; i++)
            {
                if (!slices[i].removing) { oldest = slices[i]; break; }
            }
            if (oldest == null) return;

            oldest.removing = true;
            Slice bound = oldest;
            oldest.widthEase = new CosineEase1D(time, time + slicePace, oldest.width, 0.0);
            oldest.widthEase.BindValue(v => bound.width = v);

            RetargetSlices(time);
        }

        /// <summary>
        /// Recomputes target angle and width for every non-removing slice given the
        /// current active count, and eases each from where it is now to that target
        /// over slicePace seconds. Called after every add or remove.
        /// </summary>
        private void RetargetSlices(double time)
        {
            List<Slice> active = new List<Slice>();
            for (int i = 0; i < slices.Count; i++)
            {
                if (!slices[i].removing) active.Add(slices[i]);
            }

            int n = active.Count;
            if (n == 0) return;

            double spacing = 2.0 * FireflyUtils.M_PI / n;
            double targetWidth = spacing * sliceOccupancy;

            for (int i = 0; i < n; i++)
            {
                Slice s = active[i];
                Slice bound = s;

                double targetAngleRaw = i * spacing;
                double targetAngle = s.angle + AngleDelta(targetAngleRaw, s.angle);

                s.angleEase = new CosineEase1D(time, time + slicePace, s.angle, targetAngle);
                s.angleEase.BindValue(v => bound.angle = v);

                s.widthEase = new CosineEase1D(time, time + slicePace, s.width, targetWidth);
                s.widthEase.BindValue(v => bound.width = v);
            }
        }

        private void SetUpNextAxisMove(double time)
        {
            Vector3 newCenter = stage.GetCentroid() + RandomUnitVector() * (float)stage.GetMaxRadius();
            Vector3 newUp = RandomUnitVector();
            Vector3 newPointing = RandomUnitVector();

            double endTime = time + FireflyUtils.Rand(SPIN_AXIS_MOVE_MIN, SPIN_AXIS_MOVE_MAX);

            centerEase = new CosineEase3D(time, endTime, frame.centerPos, newCenter);
            centerEase.BindValue(v => frame.centerPos = v);

            upEase = new CosineEase3D(time, endTime, frame.upAxis, newUp);
            upEase.BindValue(v => frame.upAxis = v);

            pointingEase = new CosineEase3D(time, endTime, frame.pointingAxis, newPointing);
            pointingEase.BindValue(v => frame.pointingAxis = v);
        }

        private static Vector3 RandomUnitVector()
        {
            Vector3 v;
            do
            {
                v = new Vector3(
                    (float)FireflyUtils.Rand(-1.0, 1.0),
                    (float)FireflyUtils.Rand(-1.0, 1.0),
                    (float)FireflyUtils.Rand(-1.0, 1.0));
            } while (v.sqrMagnitude < 1e-6f || v.sqrMagnitude > 1.0f);
            return v.normalized;
        }

        /// <summary>Shortest signed distance from `current` to `target`, in (-pi, pi].</summary>
        private static double AngleDelta(double target, double current)
        {
            double d = target - current;
            double twoPi = 2.0 * FireflyUtils.M_PI;
            d -= twoPi * Math.Floor((d + FireflyUtils.M_PI) / twoPi);
            return d;
        }
    }
}
