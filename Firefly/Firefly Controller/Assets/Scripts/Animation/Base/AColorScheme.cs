namespace Firefly
{
    public abstract class AColorScheme : Timeable
    {
        protected AColorPalette palette;

        protected AColorScheme() : this(new RandomSaturatedPalette()) { }

        protected AColorScheme(AColorPalette palette)
        {
            this.palette = palette;
        }

        public abstract AColorPattern NextColor();

        public virtual void SetPalette(AColorPalette palette) { this.palette = palette; }
        public virtual AColorPalette GetPalette() { return palette; }
    }
}
