using UnityEngine;

namespace Firefly
{
    /// <summary>
    /// Port of Pixel.h / .cpp. The immediate-mode drawSphere() and render() are
    /// dropped — drawing is now instanced from PixelStage. See Port Notes.
    /// </summary>
    public class Pixel
    {
        public static readonly Vector3 DEFAULT_COLOR = new Vector3(0.5f, 0.7f, 0.7f);

        // The C++ passed these to drawSphere. Unity's sphere primitive supplies the
        // mesh now, so they no longer have an effect — kept as a record of the
        // tessellation the original drew at.
        public const int PIXEL_SLICES = 8;
        public const int PIXEL_STACKS = 3;

        private Vector3 pos, color;

        public Pixel()
        {
            pos = new Vector3(0.0f, 0.0f, 0.0f);
            color = DEFAULT_COLOR;
        }

        public Pixel(Vector3 pos)
        {
            this.pos = pos;
            this.color = DEFAULT_COLOR;
        }

        public Pixel(Vector3 pos, Vector3 color)
        {
            this.pos = pos;
            this.color = color;
        }

        public void SetColor(Vector3 color) { this.color = color; }

        public Vector3 GetPos() { return pos; }
        public Vector3 GetColor() { return color; }

        public double GetX() { return pos.x; }
        public double GetY() { return pos.y; }
        public double GetZ() { return pos.z; }
    }
}
