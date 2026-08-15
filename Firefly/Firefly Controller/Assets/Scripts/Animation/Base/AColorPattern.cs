using UnityEngine;

namespace Firefly
{
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
}
