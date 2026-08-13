using System;
using System.Collections.Generic;
using UnityEngine;

namespace Firefly
{
    /// <summary>Port of SpheresAnimation.h / .cpp.</summary>
    public class SpheresAnimation : AAnimation
    {
        // SA = SpheresAnimation
        public const double SA_RINGS_PER_SECOND_AVG = 0.7;   // Rings per second average
        public const double SA_RINGS_PER_SECOND_RANGE = 0.5; // Speed will stay in average +/- this range
        public const double SA_RINGS_PER_SECOND_CYCLE = 50.0;// How long it takes speed to cycle in seconds

        public const double SA_RING_SIZE_AVG = 0.4;
        public const double SA_RING_SIZE_RANGE = 0.34;
        public const double SA_RING_SIZE_CYCLE = 43.0;

        public const double SA_CENTER_MOVE_TIME_MIN = 2.0;    // Time to move the centre point to its new location
        public const double SA_CENTER_MOVE_TIME_RANGE = 13.0; // will be in the range (MIN, MIN+RANGE)

        public const int SA_NUM_COLORS = 3;
        public const double SA_COLOR_CHANGE_TIME = 5.0;     // Crossfade length when a colour changes
        public const double SA_COLOR_CHANGE_INTERVAL = 10.0;// How often a colour is faded to a new colour
        public const int SA_COLOR_RETRIES = 5;
        public const double SA_COLOR_RANGE_THRESHOLD = 0.5;

        public const double SA_COLOR_BLEND_CYCLE = 250.0;
        public const double SA_WRAPUP_MIN_CBF = -0.75;

        private ConcentricSpheresPrim spherePrimitive = null;
        private double nextColorChange = 0.0;
        private AEasingFunction4D colorEasingFunc = null;
        private AEasingFunction3D posEasingFunc = null;

        private bool preWrapup = false, wrappingUp = false, stillRendering = true;
        private bool nextAnimationFlag = false, finishedFlag = false;
        private double ringRadiusOffset = 0.0;
        private double colorBlendOffset = 0.0;

        public SpheresAnimation(PixelStage stage)
            : this(stage, new RandomSaturatedPalette(), new SolidColorsScheme()) { }

        public SpheresAnimation(PixelStage stage, AColorPalette palette, AColorScheme colorScheme)
            : base(stage, palette, colorScheme)
        {
            subpixelSampling = FireflyUtils.Rand1() > 0.5;
        }

        protected override void InitInternal()
        {
            preWrapup = false;
            wrappingUp = false;
            stillRendering = true;
            nextAnimationFlag = false;
            finishedFlag = false;

            List<Vector4> colors = new List<Vector4>();
            while (colors.Count < SA_NUM_COLORS)
            {
                colors.Add(palette.NextColor());
            }

            ringRadiusOffset = FireflyUtils.Rand(0.0, 2 * FireflyUtils.M_PI);
            colorBlendOffset = FireflyUtils.Rand(0.0, 2 * FireflyUtils.M_PI);

            spherePrimitive = new ConcentricSpheresPrim(
                stage.GetCentroid(),
                0.0,
                0.0,
                colors,
                0.0);

            SetUpNextMove(0.0);
            SetUpNextColorChange(0.0);
        }

        protected override void UpdateInternal(double time)
        {
            spherePrimitive.sizeInRings = time * SA_RINGS_PER_SECOND_AVG;
            spherePrimitive.ringRadius =
                SA_RING_SIZE_AVG + SA_RING_SIZE_RANGE * Math.Cos(ringRadiusOffset + time / SA_RING_SIZE_CYCLE * 2 * FireflyUtils.M_PI);
            spherePrimitive.colorBlendingFactor =
                Math.Max(-0.9999, Math.Sin(colorBlendOffset + time / SA_COLOR_BLEND_CYCLE * 2 * FireflyUtils.M_PI));

            if (time > nextColorChange)
            {
                SetUpNextColorChange(time);
            }

            if (posEasingFunc.Finished())
            {
                SetUpNextMove(time);
            }

            posEasingFunc.Update(time);
            colorEasingFunc.Update(time);

            if (wrappingUp)
            {
                if (!stillRendering)
                {
                    finishedFlag = true;
                }
                else
                {
                    stillRendering = false;
                }
            }

            if (preWrapup && spherePrimitive.colorBlendingFactor > SA_WRAPUP_MIN_CBF
                && spherePrimitive.ringRadius * spherePrimitive.sizeInRings > 2.0 * stage.GetMaxRadius())
            {
                preWrapup = false;
                wrappingUp = true;
                spherePrimitive.finalRing = (int)Math.Ceiling(spherePrimitive.sizeInRings) + 3;
            }
        }

        public override Vector4 RenderPixelInternal(Vector3 pos, ArbitraryMap details)
        {
            Vector4 col = spherePrimitive.RenderPixel(pos, details);
            if (wrappingUp)
            {
                if (col.w < 1.0f)
                {
                    nextAnimationFlag = true;
                }
                if (col.w > 0.0f)
                {
                    stillRendering = true;
                }
            }
            return col;
        }

        public override void BeginWrappingUp() { preWrapup = true; }
        public override bool ReadyForNextAnimation() { return nextAnimationFlag; }
        public override bool Finished() { return finishedFlag; }

        private void SetUpNextMove(double time)
        {
            Vector3 newPos = new Vector3(
                (float)FireflyUtils.Rand(-1.0, 1.0),
                (float)FireflyUtils.Rand(-1.0, 1.0),
                (float)FireflyUtils.Rand(-1.0, 1.0));
            newPos = stage.GetCentroid() + (newPos * (float)stage.GetMaxRadius());

            double endtime = time + FireflyUtils.Rand1() * SA_CENTER_MOVE_TIME_RANGE + SA_CENTER_MOVE_TIME_MIN;

            posEasingFunc = new CosineEase3D(
                time,
                endtime,
                spherePrimitive.centerPos,
                newPos);
            posEasingFunc.BindValue(v => spherePrimitive.centerPos = v);

            Debug.Log(string.Format("[SA] Moving at t = {0:F2} to {1:F2} from {2:F2} {3:F2} {4:F2} to {5:F2} {6:F2} {7:F2}",
                time, endtime,
                spherePrimitive.centerPos.x, spherePrimitive.centerPos.y, spherePrimitive.centerPos.z,
                newPos.x, newPos.y, newPos.z));
        }

        private void SetUpNextColorChange(double time)
        {
            double colorChangeStart = time + (SA_COLOR_CHANGE_INTERVAL - SA_COLOR_CHANGE_TIME) * FireflyUtils.Rand1();
            double colorChangeEnd = colorChangeStart + SA_COLOR_CHANGE_TIME;

            int colorChangeIndex = FireflyUtils.Rand() % spherePrimitive.colors.Count;
            Vector4 bestColor = new Vector4(), nextColor;
            float bestRange = 0.0f, nextRange;
            for (int i = 0; i < SA_COLOR_RETRIES; i++)
            {
                nextColor = palette.NextColor();
                nextRange = 0.0f;
                for (int ci = 0; ci < spherePrimitive.colors.Count; ci++)
                {
                    if (ci != colorChangeIndex)
                    {
                        nextRange = Mathf.Max(nextRange, Vector4.Distance(spherePrimitive.colors[ci], nextColor));
                    }
                }
                if (nextRange > bestRange)
                {
                    bestRange = nextRange;
                    bestColor = nextColor;
                }
                if (bestRange > SA_COLOR_RANGE_THRESHOLD)
                {
                    break;
                }
            }

            colorEasingFunc = new CosineEase4D(
                colorChangeStart,
                colorChangeEnd,
                spherePrimitive.colors[colorChangeIndex],
                bestColor);
            int boundIndex = colorChangeIndex;
            colorEasingFunc.BindValue(v => spherePrimitive.colors[boundIndex] = v);

            Debug.Log(string.Format("[SA] Next color change t = {0:F2} to {1:F2}. Color blending factor {2:F2}",
                colorChangeStart, colorChangeEnd, spherePrimitive.colorBlendingFactor));

            nextColorChange = time + SA_COLOR_CHANGE_INTERVAL;
        }

        public void ShuffleColors()
        {
            for (int colorIndex = 0; colorIndex < spherePrimitive.colors.Count; colorIndex++)
                spherePrimitive.colors[colorIndex] = palette.NextColor();
        }
    }
}
