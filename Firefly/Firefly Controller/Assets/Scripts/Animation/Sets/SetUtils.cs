using System.Collections.Generic;

namespace Firefly
{
    // The C++ sets use std::map<double,int> keyed on cumulative weight, then
    // upper_bound(choice) to pick. C# SortedList gives the same ordering; the
    // upper_bound lookup is written out explicitly below.

    internal static class SetUtils
    {
        /// <summary>Equivalent of std::map::upper_bound(choice)->second.</summary>
        public static int UpperBound(SortedList<double, int> set, double choice)
        {
            IList<double> keys = set.Keys;
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i] > choice) return set[keys[i]];
            }
            return set[keys[keys.Count - 1]];
        }
    }
}
