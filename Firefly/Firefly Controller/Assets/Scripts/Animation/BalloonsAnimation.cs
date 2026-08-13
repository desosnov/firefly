using System.Collections.Generic;
using UnityEngine;

namespace Firefly
{
    /// <summary>Port of BalloonsAnimation.h / .cpp (originally WorldOfBalloonsAnimation).</summary>
    public class BalloonsAnimation : AAnimation
    {
        public const double WOB_MIN_SIZE = 0.05;
        public const double WOB_MAX_SIZE = 1.0;
        public const double WOB_MIN_GROW_TIME = 2.5;
        public const double WOB_MAX_GROW_TIME = 15.0;

        public const double WOB_STAGE_SIZE = 1.5;
        public const int WOB_SIMULT_SPHERES_GROWING = 20;
        public const int WOB_MAX_SPHERES = 100;

        public const double WOB_WRAPUP_DURATION = 10.0;
        public const double WOB_WRAPUP_MOVE_TIME = 5.0;

        public const double WOB_ALPHA_RENDER_CUTOFF = 0.97;

        protected List<SpherePrim> spheres = new List<SpherePrim>();
        protected long numSpheres = 0;
        protected List<AEasingFunction1D> growthEases = new List<AEasingFunction1D>();
        protected List<AEasingFunction3D> wrapUpEases = new List<AEasingFunction3D>();

        protected bool wrappingUp = false, nextAnimationFlag = false, finishedFlag = false;
        protected double nextAnimationTime = 0.0, finishTime = 0.0;

        public BalloonsAnimation(PixelStage stage)
            : this(stage, new RandomSaturatedPalette(), new SolidColorsScheme()) { }

        public BalloonsAnimation(PixelStage stage, AColorPalette palette, AColorScheme scheme)
            : base(stage, palette, scheme)
        {
            subpixelSampling = false;
        }

        protected override void InitInternal()
        {
            wrappingUp = false;
            nextAnimationFlag = false;
            finishedFlag = false;
            nextAnimationTime = 0.0;
            finishTime = 0.0;
            numSpheres = 0;
            spheres.Clear();
            growthEases.Clear();
            wrapUpEases.Clear();

            while (growthEases.Count < WOB_SIMULT_SPHERES_GROWING)
            {
                AddSphere(0.0);
            }
        }

        protected override void UpdateInternal(double time)
        {
            // The C++ erased from the vector while iterating it with the same iterator.
            // Iterating backwards gives the same set of removals without invalidating
            // the loop. See Port Notes.
            for (int i = growthEases.Count - 1; i >= 0; i--)
            {
                AEasingFunction1D ease = growthEases[i];
                ease.Update(time);

                if (ease.Finished() && !wrappingUp)
                {
                    growthEases.RemoveAt(i);
                    AddSphere(time);
                }
            }

            if (wrappingUp)
            {
                if (wrapUpEases.Count == 0)
                {
                    double moveStart = time, moveStartInterval = WOB_WRAPUP_DURATION / spheres.Count;
                    nextAnimationTime = time + WOB_WRAPUP_DURATION;
                    finishTime = time + WOB_WRAPUP_DURATION + WOB_WRAPUP_MOVE_TIME;

                    for (int i = spheres.Count - 1; i >= 0; i--)
                    {
                        SpherePrim sphere = spheres[i];
                        Vector3 dirFromCenter = Vector3.Normalize(sphere.centerPos - stage.GetCentroid());
                        Vector3 moveTarget = (float)((WOB_MAX_SIZE + WOB_STAGE_SIZE) * stage.GetMaxRadius()) * dirFromCenter + stage.GetCentroid();

                        CosineEase3D moveEase = new CosineEase3D(
                            moveStart,
                            moveStart + WOB_WRAPUP_MOVE_TIME,
                            sphere.centerPos,
                            moveTarget);
                        SpherePrim bound = sphere;
                        moveEase.BindValue(v => bound.centerPos = v);
                        wrapUpEases.Add(moveEase);

                        moveStart += moveStartInterval;
                    }
                }

                foreach (AEasingFunction3D ease in wrapUpEases)
                {
                    ease.Update(time);
                }

                if (time > nextAnimationTime) nextAnimationFlag = true;
                if (time > finishTime) finishedFlag = true;
            }
        }

        protected virtual void AddSphere(double time)
        {
            SpherePrim sphere = NewSphere();
            spheres.Add(sphere);
            if (spheres.Count > WOB_MAX_SPHERES)
            {
                spheres.RemoveAt(0);
            }

            AEasingFunction1D ease = NewGrowthEase(time);
            SpherePrim bound = sphere;
            ease.BindValue(v => bound.radius = v);
            growthEases.Add(ease);
        }

        protected virtual SpherePrim NewSphere()
        {
            Vector3 center;
            do
            {
                center = new Vector3(
                    (float)FireflyUtils.Rand(-1 * WOB_STAGE_SIZE, WOB_STAGE_SIZE),
                    (float)FireflyUtils.Rand(-1 * WOB_STAGE_SIZE, WOB_STAGE_SIZE),
                    (float)FireflyUtils.Rand(-1 * WOB_STAGE_SIZE, WOB_STAGE_SIZE));
            } while (center.magnitude > WOB_STAGE_SIZE);

            center = center * (float)stage.GetMaxRadius() + stage.GetCentroid();

            SpherePrim sphere = new SpherePrim(center, 0.0, colorScheme.NextColor(), (int)numSpheres);
            numSpheres++;
            return sphere;
        }

        protected virtual AEasingFunction1D NewGrowthEase(double time)
        {
            CosineEase1D ease = new CosineEase1D(
                time,
                time + FireflyUtils.Rand(WOB_MIN_GROW_TIME, WOB_MAX_GROW_TIME),
                0.0,
                FireflyUtils.Rand(WOB_MIN_SIZE, WOB_MAX_SIZE) * stage.GetMaxRadius());
            return ease;
        }

        public override Vector4 RenderPixelInternal(Vector3 pos, ArbitraryMap details)
        {
            Vector4 pixelColor = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            Vector4 renderColor;

            for (int i = spheres.Count - 1; i >= 0; i--)
            {
                renderColor = spheres[i].RenderPixel(pos, details);
                if (renderColor.w == 0.0f)
                {
                    continue;
                }

                pixelColor = BlendColors(renderColor, pixelColor);

                if (pixelColor.w >= WOB_ALPHA_RENDER_CUTOFF)
                {
                    break;
                }
            }

            return pixelColor;
        }

        public override void BeginWrappingUp() { wrappingUp = true; }
        public override bool ReadyForNextAnimation() { return nextAnimationFlag; }
        public override bool Finished() { return finishedFlag; }
    }

    /// <summary>Port of BalloonsAtPixelsAnimation.h / .cpp.</summary>
    public class BalloonsAtPixelsAnimation : BalloonsAnimation
    {
        public BalloonsAtPixelsAnimation(PixelStage stage)
            : this(stage, new RandomSaturatedPalette(), new SolidColorsScheme()) { }

        public BalloonsAtPixelsAnimation(PixelStage stage, AColorPalette palette, AColorScheme scheme)
            : base(stage, palette, scheme)
        {
            subpixelSampling = false;
        }

        protected override SpherePrim NewSphere()
        {
            int pixel = FireflyUtils.Rand() % stage.pixelsLen;
            Vector3 center = stage.pixels[pixel].GetPos();
            Vector3 offset = new Vector3(
                (float)FireflyUtils.Rand(-1.0, 1.0),
                (float)FireflyUtils.Rand(-1.0, 1.0),
                (float)FireflyUtils.Rand(-1.0, 1.0)) * (float)stage.GetPixelRadius();
            center += offset;

            SpherePrim sphere = new SpherePrim(center, 0.0, colorScheme.NextColor(), (int)numSpheres);
            numSpheres++;
            return sphere;
        }
    }
}
