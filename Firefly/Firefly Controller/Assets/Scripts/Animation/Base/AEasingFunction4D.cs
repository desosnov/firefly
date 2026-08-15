using System;
using System.Collections.Generic;
using UnityEngine;

namespace Firefly
{
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
}
