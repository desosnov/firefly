using System;
using UnityEngine;

namespace Firefly
{
    /// <summary>
    /// Port of Camera.h / .cpp. Renamed from Camera to avoid colliding with
    /// UnityEngine.Camera. gluLookAt is replaced by positioning Unity's camera
    /// transform. The orbital maths is unchanged apart from the up axis: the C++
    /// was Z-up, this is Y-up. See Port Notes.
    /// </summary>
    public class FireflyCamera
    {
        public const double DEFAULT_HOR = 45.0;
        public const double DEFAULT_VER = 45.0;
        public const double DEFAULT_X = 0.0;
        public const double DEFAULT_Y = 0.0;
        public const double DEFAULT_Z = 0.0;
        public const double DEFAULT_DIST = 3.5;
        public const double MIN_DIST = 1.5;
        public const double MAX_DIST = 10.0;
        public const double ZOOM_MULT = 1.1;
        public const double MAX_VERTICAL = 80.0;
        public const double MIN_VERTICAL = -80.0;

        public const double PI = 3.14159265359;

        private double hor = DEFAULT_HOR, ver = DEFAULT_VER;
        private Vector3 pos = new Vector3((float)DEFAULT_X, (float)DEFAULT_Y, (float)DEFAULT_Z);
        private double dist = DEFAULT_DIST;

        public Vector3 GetPos() { return pos; }

        /// <summary>
        /// The C++ called gluLookAt with an eye position derived from hor/ver/dist
        /// and (0,0,1) as up. Same orbit, with Y as the up axis: hor sweeps the
        /// horizontal circle in x/z, ver raises the eye in y.
        /// </summary>
        public void ApplyTo(Camera unityCamera)
        {
            Vector3 eye = new Vector3(
                (float)(pos.x + dist * Math.Cos(hor * PI / 180.0) * Math.Cos(ver * PI / 180.0)),
                (float)(pos.y + dist * Math.Sin(ver * PI / 180.0)),
                (float)(pos.z + dist * Math.Sin(hor * PI / 180.0) * Math.Cos(ver * PI / 180.0)));

            unityCamera.transform.position = eye;
            unityCamera.transform.LookAt(pos, Vector3.up);
        }

        public void Rotate(double horiz_move, double vert_move)
        {
            hor += horiz_move;
            ver += vert_move;

            if (ver > MAX_VERTICAL) ver = MAX_VERTICAL;
            if (ver < MIN_VERTICAL) ver = MIN_VERTICAL;
        }

        public void MoveTo(Vector3 newPos) { pos = newPos; }

        public void Zoom(double distCloser)
        {
            dist -= distCloser;
            if (dist < MIN_DIST) dist = MIN_DIST;
            if (dist > MAX_DIST) dist = MAX_DIST;
        }

        public void ZoomIn()
        {
            dist /= ZOOM_MULT;
            if (dist < MIN_DIST) dist = MIN_DIST;
            if (dist > MAX_DIST) dist = MAX_DIST;
        }

        public void ZoomOut()
        {
            dist *= ZOOM_MULT;
            if (dist < MIN_DIST) dist = MIN_DIST;
            if (dist > MAX_DIST) dist = MAX_DIST;
        }
    }
}
