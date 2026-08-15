namespace Firefly
{
    public class SolidColorsScheme : AColorScheme
    {
        public SolidColorsScheme() : base(new RandomSaturatedPalette()) { }
        public SolidColorsScheme(AColorPalette palette) : base(palette) { }

        public override AColorPattern NextColor()
        {
            return new SolidPattern(palette.NextColor());
        }
    }
}
