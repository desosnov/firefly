using System;
using UnityEngine;

namespace Firefly
{
    public class CosineEase3D : AEasingFunction3D
    {
        public CosineEase3D(double start, double finish, Vector3 easeFrom, Vector3 easeTo)
            : base(start, finish, easeFrom, easeTo) { }

        public override Vector3 EasedValue(double input)
        {
            double norm = NormalizeInput(input);
            return easeFrom + (easeTo - easeFrom) * (float)(-0.5 * Math.Cos(norm * FireflyUtils.M_PI) + 0.5);
        }
    }
}
