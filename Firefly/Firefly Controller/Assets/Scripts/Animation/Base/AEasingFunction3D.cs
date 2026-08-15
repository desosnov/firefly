using System;
using System.Collections.Generic;
using UnityEngine;

namespace Firefly
{
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
}
