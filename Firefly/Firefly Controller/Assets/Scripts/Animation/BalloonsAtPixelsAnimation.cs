using UnityEngine;

namespace Firefly
{
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
