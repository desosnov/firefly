namespace Firefly
{
    public class RandomGradientScheme : AColorScheme
    {
        public RandomGradientScheme() : base(new RandomSaturatedPalette()) { }
        public RandomGradientScheme(AColorPalette palette) : base(palette) { }

        public override AColorPattern NextColor()
        {
            return new TwoColorGradientPattern(palette.RandomColor(), palette.RandomColor());
        }
    }
}
