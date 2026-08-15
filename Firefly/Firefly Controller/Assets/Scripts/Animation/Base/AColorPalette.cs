using UnityEngine;

namespace Firefly
{
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
}
