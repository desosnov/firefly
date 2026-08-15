using System.Collections.Generic;
using UnityEngine;

namespace Firefly
{
    public abstract class ColorSchemeSet
    {
        protected SortedList<double, int> weightedSet = new SortedList<double, int>();
        protected double totalWeight = 0.0;

        protected void Add(int creationIndex, double weight)
        {
            weightedSet[weight + totalWeight] = creationIndex;
            totalWeight += weight;
        }

        protected abstract AColorScheme CreateIndex(int index);

        public AColorScheme Next()
        {
            if (weightedSet.Count == 0) return null;

            double choice = FireflyUtils.Rand1() * totalWeight;
            return CreateIndex(SetUtils.UpperBound(weightedSet, choice));
        }
    }

    public class AllColorSchemes : ColorSchemeSet
    {
        public AllColorSchemes()
        {
            Add(0, 1.0); // SolidColorsScheme
            Add(1, 2.0); // RandomGradientScheme
            Add(2, 2.0); // GradientToBlackScheme
            Add(3, 2.0); // GradientToTransparentScheme
        }

        protected override AColorScheme CreateIndex(int index)
        {
            switch (index)
            {
                case 0: Debug.Log("[AllColorSchemes] Return SolidColorsScheme"); return new SolidColorsScheme();
                case 1: Debug.Log("[AllColorSchemes] Return RandomGradientScheme"); return new RandomGradientScheme();
                case 2: Debug.Log("[AllColorSchemes] Return GradientToBlackScheme"); return new GradientToBlackScheme();
                case 3: Debug.Log("[AllColorSchemes] Return GradientToTransparentScheme"); return new GradientToTransparentScheme();
                default: Debug.Log("[AllColorSchemes] Return default SolidColorsScheme"); return new SolidColorsScheme();
            }
        }
    }
}
