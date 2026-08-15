using System;

namespace Firefly
{
    public class CosineEase1D : AEasingFunction1D
    {
        public CosineEase1D(double start, double finish, double easeFrom, double easeTo)
            : base(start, finish, easeFrom, easeTo) { }

        public override double EasedValue(double input)
        {
            double norm = NormalizeInput(input);
            return easeFrom + (easeTo - easeFrom) * (-0.5 * Math.Cos(norm * FireflyUtils.M_PI) + 0.5);
        }
    }
}
