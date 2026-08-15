using UnityEngine;

namespace Firefly
{
    public abstract class APixelShader
    {
        protected int id;

        protected APixelShader(int id = 0)
        {
            this.id = id;
        }

        public abstract Vector4 RenderPixel(Vector3 pos, Vector4 color, ArbitraryMap details);
    }
}
