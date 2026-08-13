using UnityEngine;

namespace Firefly
{
    /// <summary>Port of AColorPalette.h / .cpp.</summary>
    public abstract class AColorPalette : Timeable
    {
        public const double ACP_LAST_COLOR_MIN_DISTANCE = 0.2;

        protected Vector4 lastColor;

        protected Vector4 CheckAgainstLastColor(Vector4 color)
        {
            if (Vector4.Distance(color, lastColor) < ACP_LAST_COLOR_MIN_DISTANCE)
            {
                return NextColor();
            }
            else
            {
                lastColor = color;
                return color;
            }
        }

        public abstract Vector4 NextColor();

        public virtual Vector4 RandomColor()
        {
            if (NumColors() == -1)
            {
                return NextColor();
            }
            else
            {
                for (int i = FireflyUtils.Rand() % NumColors(); i >= 0; i--)
                {
                    NextColor();
                }
                return NextColor();
            }
        }

        /// <summary>
        /// -1 if this palette will return random new colors infinitely.
        /// N if this is a palette of a static N colors. It is expected they repeat in order.
        /// </summary>
        public virtual int NumColors() { return -1; }
    }

    /// <summary>Port of AColorPattern.h / .cpp.</summary>
    public abstract class AColorPattern : Timeable
    {
        public Vector4 color;

        protected AColorPattern() : this(new Vector4(1.0f, 1.0f, 1.0f, 1.0f)) { }

        protected AColorPattern(Vector4 color)
        {
            this.color = color;
        }

        public abstract Vector4 GetColor();

        public virtual Vector4 GetColor(double x)
        {
            return GetColor();
        }

        public virtual Vector4 GetColor(double x, double y)
        {
            return GetColor(x);
        }

        public virtual Vector4 GetColor(Vector3 pos)
        {
            return GetColor();
        }

        public virtual Vector4 GetColor(double x, Vector3 pos)
        {
            if (GetColor(x) != GetColor()) return GetColor(x);
            return GetColor(pos);
        }

        public virtual Vector4 GetColor(double x, double y, Vector3 pos)
        {
            if (GetColor(x, y) != GetColor()) return GetColor(x, y);
            return GetColor(pos);
        }
    }

    /// <summary>Port of AColorScheme.h / .cpp.</summary>
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
