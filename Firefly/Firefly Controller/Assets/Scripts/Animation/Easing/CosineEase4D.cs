using System;
using UnityEngine;

namespace Firefly
{
    public class CosineEase4D : AEasingFunction4D
    {
        public CosineEase4D(double start, double finish, Vector4 easeFrom, Vector4 easeTo)
            : base(start, finish, easeFrom, easeTo) { }

        public override Vector4 EasedValue(double input)
        {
            double norm = NormalizeInput(input);
            return easeFrom + (easeTo - easeFrom) * (float)(-0.5 * Math.Cos(norm * FireflyUtils.M_PI) + 0.5);
        }
    }
}
