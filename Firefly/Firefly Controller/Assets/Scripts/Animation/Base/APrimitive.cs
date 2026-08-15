using UnityEngine;

namespace Firefly
{
    public abstract class APrimitive
    {
        protected abstract Vector4 RenderPixelAt(Vector3 pos, ArbitraryMap details);

        public Vector4 RenderPixel(Vector3 pos, ArbitraryMap details)
        {
            return RenderPixelAt(pos, details);
        }
    }
}
