using System;
using System.Collections.Generic;

namespace Firefly
{
    // The C++ easing functions bind to a variable by raw pointer — bindValue(double*)
    // then write through it on update. C# has no equivalent, so bindings are setter
    // delegates instead. Same semantics: bind once, call Update(time), the target
    // updates itself. See Port Notes.

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
}
