using UnityEngine;

namespace Firefly
{
    // ── Patterns ────────────────────────────────────────────────

    /// <summary>Port of SolidPattern.h / .cpp.</summary>
    public class SolidPattern : AColorPattern
    {
        public SolidPattern() : base() { }
        public SolidPattern(Vector4 color) : base(color) { }

        public override Vector4 GetColor() { return color; }
    }

    /// <summary>Port of TwoColorGradientPattern.h / .cpp.</summary>
    public class TwoColorGradientPattern : AColorPattern
    {
        public Vector4 color2;

        public TwoColorGradientPattern()
            : this(new Vector4(1.0f, 1.0f, 1.0f, 1.0f), new Vector4(0.0f, 0.0f, 0.0f, 1.0f)) { }

        public TwoColorGradientPattern(Vector4 color, Vector4 color2) : base(color)
        {
            this.color2 = color2;
        }

        public override Vector4 GetColor() { return color; }

        public override Vector4 GetColor(double x)
        {
            return (float)(1.0 - x) * color + (float)x * color2;
        }
    }

    // ── Schemes ─────────────────────────────────────────────────

    /// <summary>Port of SolidColorsScheme.h / .cpp.</summary>
    public class SolidColorsScheme : AColorScheme
    {
        public SolidColorsScheme() : base(new RandomSaturatedPalette()) { }
        public SolidColorsScheme(AColorPalette palette) : base(palette) { }

        public override AColorPattern NextColor()
        {
            return new SolidPattern(palette.NextColor());
        }
    }

    /// <summary>Port of RandomGradientScheme.h / .cpp.</summary>
    public class RandomGradientScheme : AColorScheme
    {
        public RandomGradientScheme() : base(new RandomSaturatedPalette()) { }
        public RandomGradientScheme(AColorPalette palette) : base(palette) { }

        public override AColorPattern NextColor()
        {
            return new TwoColorGradientPattern(palette.RandomColor(), palette.RandomColor());
        }
    }

    /// <summary>Port of GradientToBlackScheme.h / .cpp.</summary>
    public class GradientToBlackScheme : AColorScheme
    {
        public GradientToBlackScheme() : base(new RandomSaturatedPalette()) { }
        public GradientToBlackScheme(AColorPalette palette) : base(palette) { }

        public override AColorPattern NextColor()
        {
            return new TwoColorGradientPattern(palette.RandomColor(), new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
        }
    }

    /// <summary>Port of GradientToTransparentScheme.h / .cpp.</summary>
    public class GradientToTransparentScheme : AColorScheme
    {
        public GradientToTransparentScheme() : base(new RandomSaturatedPalette()) { }
        public GradientToTransparentScheme(AColorPalette palette) : base(palette) { }

        public override AColorPattern NextColor()
        {
            Vector4 color = palette.RandomColor();
            Vector4 color2 = color;
            color2.w = 0.0f;
            return new TwoColorGradientPattern(color, color2);
        }
    }
}
