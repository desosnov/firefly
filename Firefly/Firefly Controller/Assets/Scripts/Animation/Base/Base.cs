using System.Collections.Generic;
using UnityEngine;

namespace Firefly
{
    /// <summary>Port of Timeable.h / .cpp.</summary>
    public class Timeable
    {
        public virtual void Init(double time) { }
        public virtual void Update(double time) { }
    }

    /// <summary>
    /// Port of ArbitraryMap. The C++ keyed a std::map on char* — i.e. on pointer
    /// identity, not string content. C# keys on content, which is what the original
    /// intended. See Port Notes.
    /// </summary>
    public class ArbitraryMap
    {
        private readonly Dictionary<string, object> arbMap = new Dictionary<string, object>();

        public bool HasKey(string key) { return arbMap.ContainsKey(key); }

        public void SetInt(string key, int val) { arbMap[key] = val; }
        public int GetInt(string key) { return (int)arbMap[key]; }

        public void SetDouble(string key, double val) { arbMap[key] = val; }
        public double GetDouble(string key) { return (double)arbMap[key]; }

        public void SetBool(string key, bool val) { arbMap[key] = val; }
        public bool GetBool(string key) { return (bool)arbMap[key]; }

        public void SetPtr(string key, object val) { arbMap[key] = val; }
        public object GetPtr(string key) { return arbMap[key]; }
    }

    /// <summary>Port of APrimitive.h / .cpp.</summary>
    public abstract class APrimitive
    {
        protected abstract Vector4 RenderPixelAt(Vector3 pos, ArbitraryMap details);

        public Vector4 RenderPixel(Vector3 pos, ArbitraryMap details)
        {
            return RenderPixelAt(pos, details);
        }
    }

    /// <summary>Port of APixelShader.h / .cpp.</summary>
    public abstract class APixelShader
    {
        protected int id;

        protected APixelShader(int id = 0)
        {
            this.id = id;
        }

        public abstract Vector4 RenderPixel(Vector3 pos, Vector4 color, ArbitraryMap details);
    }

    /// <summary>Port of IntervalSlicerWithSymmetricalPhases.h / .cpp.</summary>
    public class IntervalSlicerWithSymmetricalPhases
    {
        public double center;
        public double interval;

        public IntervalSlicerWithSymmetricalPhases(double center, double intervalSize)
        {
            this.center = center;
            this.interval = intervalSize;
        }

        public int GetInterval(double point)
        {
            return (int)System.Math.Floor((point - center) / interval);
        }

        public double GetPhase(double point)
        {
            // Normalize to a value from 0 to 1 across the interval
            return ((point - center) % interval) / interval;
        }

        public double GetSymmetricalPhase(double point)
        {
            double phase = GetPhase(point);
            phase -= 0.5;                       // Shift to (-0.5, 0.5)
            phase *= 2;                         // Shift to (-1.0, 1.0)
            phase = 1.0 - System.Math.Abs(phase); // Absolute value, inverted so centre is 1.0
            return phase;
        }
    }
}
