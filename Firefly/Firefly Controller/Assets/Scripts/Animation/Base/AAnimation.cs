using System.Collections.Generic;
using UnityEngine;

namespace Firefly
{
    public abstract class AAnimation
    {
        public const double SUBPIXEL_RADIUS_RATIO = 1.0;
        public const double SUBPIXEL_ORIGINAL_WEIGHT = 0.25;
        public const double SUBPIXEL_WEIGHT = (1.0 - SUBPIXEL_ORIGINAL_WEIGHT) / 6.0;

        protected PixelStage stage;
        protected AColorPalette palette;
        protected AColorScheme colorScheme;
        protected List<ArbitraryMap> pixelDetails = new List<ArbitraryMap>();
        protected List<APixelShader> pixelShaders = new List<APixelShader>();

        protected double startTime = 0.0;
        protected bool subpixelSampling = false;
        protected double subpixelDist;

        protected AAnimation(PixelStage pixelStage)
            : this(pixelStage, new RandomSaturatedPalette(), new SolidColorsScheme()) { }

        protected AAnimation(PixelStage pixelStage, AColorPalette palette, AColorScheme colorScheme)
        {
            this.stage = pixelStage;
            this.palette = palette;
            this.colorScheme = colorScheme;

            for (int p = 0; p < stage.pixelsLen; p++)
            {
                ArbitraryMap map = new ArbitraryMap();
                map.SetInt("pixelIndex", p);
                pixelDetails.Add(map);
            }
            subpixelDist = pixelStage.GetPixelRadius() * SUBPIXEL_RADIUS_RATIO;
        }

        protected abstract void InitInternal();
        protected abstract void UpdateInternal(double time);
        public abstract Vector4 RenderPixelInternal(Vector3 pixelPos, ArbitraryMap details);

        public abstract void BeginWrappingUp();
        public abstract bool ReadyForNextAnimation();
        public abstract bool Finished();

        protected virtual Vector4 BlendColors(Vector4 underColor, Vector4 overColor)
        {
            float outAlpha = overColor.w + underColor.w * (1.0f - overColor.w);
            if (outAlpha == 0.0f)
            {
                return new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            }

            Vector3 over = new Vector3(overColor.x, overColor.y, overColor.z);
            Vector3 under = new Vector3(underColor.x, underColor.y, underColor.z);
            Vector3 outRGB = (over * overColor.w + (1.0f - overColor.w) * under * underColor.w) / outAlpha;
            return new Vector4(outRGB.x, outRGB.y, outRGB.z, outAlpha);
        }

        public void Init(double time)
        {
            startTime = time;
            InitInternal();
        }

        public void Update(double time)
        {
            UpdateInternal(time - startTime);
        }

        public void Render(double time)
        {
            UpdateInternal(time - startTime);

            for (int p = 0; p < stage.pixelsLen; p++)
            {
                pixelDetails[p].SetInt("shaderIndex", -1);
                Vector4 pixelColor = RenderPixel(stage.pixels[p].GetPos(), pixelDetails[p]);
                stage.pixels[p].SetColor(new Vector3(
                    pixelColor.x * pixelColor.w,
                    pixelColor.y * pixelColor.w,
                    pixelColor.z * pixelColor.w));
            }
        }

        public Vector4 RenderPixel(Vector3 pixelPos, ArbitraryMap details)
        {
            Vector4 pixelColor;

            if (subpixelSampling)
            {
                Vector3 subpixelPos = pixelPos;
                float d = (float)subpixelDist;

                subpixelPos.x += d;
                pixelColor = RenderPixelInternal(subpixelPos, details);
                subpixelPos.x -= d * 2;
                pixelColor += RenderPixelInternal(subpixelPos, details);
                subpixelPos.x += d;

                subpixelPos.y += d;
                pixelColor += RenderPixelInternal(subpixelPos, details);
                subpixelPos.y -= d * 2;
                pixelColor += RenderPixelInternal(subpixelPos, details);
                subpixelPos.y += d;

                subpixelPos.z += d;
                pixelColor += RenderPixelInternal(subpixelPos, details);
                subpixelPos.z -= d * 2;
                pixelColor += RenderPixelInternal(subpixelPos, details);
                subpixelPos.z += d;

                pixelColor *= (float)SUBPIXEL_WEIGHT;
                pixelColor += (float)SUBPIXEL_ORIGINAL_WEIGHT * RenderPixelInternal(pixelPos, details);
            }
            else
            {
                pixelColor = RenderPixelInternal(pixelPos, details);
            }

            for (int s = 0; s < pixelShaders.Count; s++)
            {
                if (pixelShaders[s] != null)
                {
                    pixelColor = pixelShaders[s].RenderPixel(pixelPos, pixelColor, details);
                }
            }

            return pixelColor;
        }

        public void AddShader(APixelShader shader)
        {
            pixelShaders.Add(shader);
        }

        public void ResetShaders()
        {
            pixelShaders.Clear();
        }

        public void SetColorScheme(AColorScheme newScheme) { colorScheme = newScheme; }
        public void SetColorPalette(AColorPalette newPalette) { palette = newPalette; }

        public bool ToggleSubpixelSampling()
        {
            subpixelSampling = !subpixelSampling;
            FireflyUtils.Log("ANIM Set subpixel sampling to " + subpixelSampling);
            return subpixelSampling;
        }
    }
}
