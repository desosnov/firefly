using UnityEngine;

namespace Firefly
{
    public class SolidPattern : AColorPattern
    {
        public SolidPattern() : base() { }
        public SolidPattern(Vector4 color) : base(color) { }

        public override Vector4 GetColor() { return color; }
    }
}
