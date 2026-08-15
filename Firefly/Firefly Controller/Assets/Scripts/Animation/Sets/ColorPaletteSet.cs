using System.Collections.Generic;
using UnityEngine;

namespace Firefly
{
    public abstract class ColorPaletteSet
    {
        protected SortedList<double, int> weightedSet = new SortedList<double, int>();
        protected double totalWeight = 0.0;

        protected void Add(int creationIndex, double weight)
        {
            weightedSet[weight + totalWeight] = creationIndex;
            totalWeight += weight;
        }

        protected abstract AColorPalette CreateIndex(int index);

        public AColorPalette Next()
        {
            if (weightedSet.Count == 0) return null;

            double choice = FireflyUtils.Rand1() * totalWeight;
            return CreateIndex(SetUtils.UpperBound(weightedSet, choice));
        }
    }

    public class AllColorPalettes : ColorPaletteSet
    {
        public AllColorPalettes()
        {
            Add(0, 2.0); // RandomSaturatedPalette
            Add(1, 2.0); // RandomDesaturatedPalette
            Add(2, 1.0); // SingleRandomHuePalette
            Add(3, 2.0); // TwoRandomHuesPalette
            Add(4, 2.0); // NarrowHueRangePalette
            Add(5, 2.0); // WideHueRangePalette
        }

        protected override AColorPalette CreateIndex(int index)
        {
            switch (index)
            {
                case 0: Debug.Log("[AllColorPalettes] Return RandomSaturatedPalette"); return new RandomSaturatedPalette();
                case 1: Debug.Log("[AllColorPalettes] Return RandomDesaturatedPalette"); return new RandomDesaturatedPalette();
                case 2: Debug.Log("[AllColorPalettes] Return SingleRandomHuePalette"); return new SingleRandomHuePalette();
                case 3: Debug.Log("[AllColorPalettes] Return TwoRandomHuesPalette"); return new TwoRandomHuesPalette();
                case 4: Debug.Log("[AllColorPalettes] Return NarrowHueRangePalette"); return new NarrowHueRangePalette();
                case 5: Debug.Log("[AllColorPalettes] Return WideHueRangePalette"); return new WideHueRangePalette();
                default: Debug.Log("[AllColorPalettes] Return default RandomSaturatedPalette"); return new RandomSaturatedPalette();
            }
        }
    }
}
