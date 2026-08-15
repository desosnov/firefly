using System.Collections.Generic;

namespace Firefly
{
    /// <summary>
    /// The C++ keyed a std::map on char* — i.e. on pointer identity, not string
    /// content. C# keys on content, which is what the original intended. See Port Notes.
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
}
