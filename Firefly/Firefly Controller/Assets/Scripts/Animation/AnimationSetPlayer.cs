using UnityEngine;

namespace Firefly
{
    /// <summary>Port of AnimationSetPlayer.h / .cpp.</summary>
    public class AnimationSetPlayer : AAnimation
    {
        public const int ASP_MAX_SHADERS = 5;

        private AnimationSet animations;
        private AAnimation curAnim = null, nextAnim = null;
        private PixelShaderSet pixelShaders;
        private ColorPaletteSet palettes;
        private ColorSchemeSet colorSchemes;

        private double minDuration, maxDuration, nextSwitch;

        public AnimationSetPlayer(
            PixelStage stage,
            AnimationSet animations,
            PixelShaderSet pixelShaders = null,
            ColorPaletteSet palettes = null,
            ColorSchemeSet colorSchemes = null,
            double minDuration = 30.0,
            double maxDuration = 300.0)
            : base(stage)
        {
            this.animations = animations;
            this.pixelShaders = pixelShaders ?? new AllPixelShaders();
            this.palettes = palettes ?? new AllColorPalettes();
            this.colorSchemes = colorSchemes ?? new AllColorSchemes();
            this.minDuration = minDuration;
            this.maxDuration = maxDuration;
        }

        protected override void InitInternal()
        {
            curAnim = BuildNextAnimation();
            curAnim.Init(0.0);

            nextSwitch = FireflyUtils.Rand(minDuration, maxDuration);
            FireflyUtils.Log("[ASP] Initialized animation set player with next switch at " + nextSwitch);
        }

        protected override void UpdateInternal(double time)
        {
            if (subpixelSampling)
            {
                subpixelSampling = false;
                curAnim.ToggleSubpixelSampling();
            }

            if (time > nextSwitch && nextAnim == null)
            {
                curAnim.BeginWrappingUp();
                nextSwitch += 1000.0;
                FireflyUtils.Log("[ASP] Hit switch time, wrapping up cur animation");
            }

            if (nextAnim == null && curAnim.ReadyForNextAnimation())
            {
                nextAnim = BuildNextAnimation();
                nextAnim.Init(time);

                nextSwitch = time + FireflyUtils.Rand(minDuration, maxDuration);
                FireflyUtils.Log("[ASP] Cur animation is ready for next animation, starting next animation");
                FireflyUtils.Log("[ASP] Next switch: " + nextSwitch);
            }

            if (curAnim.Finished())
            {
                curAnim = nextAnim;
                nextAnim = null;
                FireflyUtils.Log("[ASP] Cur animation is finished, replacing with next animation");
                FireflyUtils.Log("[ASP] Next switch: " + nextSwitch);
            }

            if (nextAnim != null)
            {
                nextAnim.Update(time);
            }
            curAnim.Update(time);
        }

        public override Vector4 RenderPixelInternal(Vector3 pos, ArbitraryMap details)
        {
            Vector4 color = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            if (nextAnim != null)
            {
                color = nextAnim.RenderPixel(pos, details);
            }

            color = BlendColors(color, curAnim.RenderPixel(pos, details));
            return color;
        }

        // The C++ bodies for these three are empty; the latter two fall off the end
        // without returning a value. Nothing calls them — AnimationSetPlayer is never
        // nested inside another player. See Port Notes.
        public override void BeginWrappingUp() { }
        public override bool ReadyForNextAnimation() { return false; }
        public override bool Finished() { return false; }

        private void RandomizeShaders(AAnimation anim)
        {
            anim.ResetShaders();
            int shaders = FireflyUtils.Rand() % (ASP_MAX_SHADERS + 1);
            shaders = 1;
            Debug.Log(string.Format("[ASP] Next animation has {0} shaders", shaders));
            for (int shader = 0; shader < shaders; shader++)
            {
                anim.AddShader(pixelShaders.Next());
            }
        }

        private AAnimation BuildNextAnimation()
        {
            AAnimation newAnim = animations.Next();

            AColorPalette palette = palettes.Next();
            AColorScheme scheme = colorSchemes.Next();
            scheme.SetPalette(palette);
            newAnim.SetColorPalette(palette);
            newAnim.SetColorScheme(scheme);

            RandomizeShaders(newAnim);

            return newAnim;
        }
    }
}
