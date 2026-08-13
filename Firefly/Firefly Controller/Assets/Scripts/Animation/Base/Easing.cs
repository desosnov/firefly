using System;
using System.Collections.Generic;
using UnityEngine;

namespace Firefly
{
    // The C++ easing functions bind to a variable by raw pointer — bindValue(double*)
    // then write through it on update. C# has no equivalent, so bindings are setter
    // delegates instead. Same semantics: bind once, call Update(time), the target
    // updates itself. See Port Notes.

    /// <summary>Port of AEasingFunction1D.h / .cpp.</summary>
    public abstract class AEasingFunction1D
    {
        protected List<Action<double>> bindings = new List<Action<double>>();
        protected bool finishedFlag = false;

        public double start, finish;
        public double easeFrom, easeTo;

        protected AEasingFunction1D(double start, double finish, double easeFrom, double easeTo)
        {
            this.start = start;
            this.finish = finish;
            this.easeFrom = easeFrom;
            this.easeTo = easeTo;
        }

        protected double NormalizeInput(double input)
        {
            double normalized = (input - start) / (finish - start);
            return Math.Max(Math.Min(normalized, 1.0), 0.0);
        }

        public void BindValue(Action<double> setter)
        {
            if (!bindings.Contains(setter)) bindings.Add(setter);
        }

        public void Update(double time)
        {
            foreach (Action<double> setter in bindings) setter(EasedValue(time));

            finishedFlag = (time - start) / (finish - start) > 1.0;
        }

        public bool Finished() { return finishedFlag; }

        public abstract double EasedValue(double input);
    }

    /// <summary>Port of AEasingFunction3D.h / .cpp.</summary>
    public abstract class AEasingFunction3D
    {
        protected List<Action<Vector3>> bindings = new List<Action<Vector3>>();
        protected bool finishedFlag = false;

        public double start, finish;
        public Vector3 easeFrom, easeTo;

        protected AEasingFunction3D(double start, double finish, Vector3 easeFrom, Vector3 easeTo)
        {
            this.start = start;
            this.finish = finish;
            this.easeFrom = easeFrom;
            this.easeTo = easeTo;
        }

        protected double NormalizeInput(double input)
        {
            double normalized = (input - start) / (finish - start);
            return Math.Max(Math.Min(normalized, 1.0), 0.0);
        }

        public void BindValue(Action<Vector3> setter)
        {
            if (!bindings.Contains(setter)) bindings.Add(setter);
        }

        public void Update(double time)
        {
            foreach (Action<Vector3> setter in bindings) setter(EasedValue(time));

            finishedFlag = (time - start) / (finish - start) > 1.0;
        }

        public bool Finished() { return finishedFlag; }

        public abstract Vector3 EasedValue(double input);
    }

    /// <summary>Port of AEasingFunction4D.h / .cpp.</summary>
    public abstract class AEasingFunction4D
    {
        protected List<Action<Vector4>> bindings = new List<Action<Vector4>>();
        protected bool finishedFlag = false;

        public double start, finish;
        public Vector4 easeFrom, easeTo;

        protected AEasingFunction4D(double start, double finish, Vector4 easeFrom, Vector4 easeTo)
        {
            this.start = start;
            this.finish = finish;
            this.easeFrom = easeFrom;
            this.easeTo = easeTo;
        }

        protected double NormalizeInput(double input)
        {
            double normalized = (input - start) / (finish - start);
            return Math.Max(Math.Min(normalized, 1.0), 0.0);
        }

        public void BindValue(Action<Vector4> setter)
        {
            if (!bindings.Contains(setter)) bindings.Add(setter);
        }

        public void Update(double time)
        {
            foreach (Action<Vector4> setter in bindings) setter(EasedValue(time));

            finishedFlag = (time - start) / (finish - start) > 1.0;
        }

        public bool Finished() { return finishedFlag; }

        public abstract Vector4 EasedValue(double input);
    }

    /// <summary>Port of CosineEase1D.h / .cpp.</summary>
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

    /// <summary>Port of CosineEase3D.h / .cpp.</summary>
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

    /// <summary>Port of CosineEase4D.h / .cpp.</summary>
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
