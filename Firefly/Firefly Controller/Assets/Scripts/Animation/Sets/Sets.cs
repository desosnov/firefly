using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Firefly
{
    // The C++ sets use std::map<double,int> keyed on cumulative weight, then
    // upper_bound(choice) to pick. C# SortedList gives the same ordering; the
    // upper_bound lookup is written out explicitly below.

    /// <summary>Port of AnimationSet.h / .cpp.</summary>
    public abstract class AnimationSet
    {
        protected PixelStage stage;
        protected SortedList<double, int> weightedSet = new SortedList<double, int>();
        protected double totalWeight = 0.0;

        protected AnimationSet(PixelStage stage)
        {
            this.stage = stage;
        }

        protected void Add(int creationIndex, double weight)
        {
            weightedSet[weight + totalWeight] = creationIndex;
            totalWeight += weight;
        }

        protected abstract AAnimation CreateIndex(int index);

        public AAnimation Next()
        {
            if (weightedSet.Count == 0) return null;

            double choice = FireflyUtils.Rand1() * totalWeight;
            return CreateIndex(SetUtils.UpperBound(weightedSet, choice));
        }
    }

    public class AllAnimations : AnimationSet
    {
        public AllAnimations(PixelStage stage) : base(stage)
        {
            Add(0, 3.0); // SpheresAnimation
            Add(1, 1.0); // BalloonsAnimation
            Add(2, 1.0); // BalloonsAtPixelsAnimation
        }

        protected override AAnimation CreateIndex(int index)
        {
            switch (index)
            {
                case 0: Debug.Log("[AllAnimations] Return SpheresAnimation"); return new SpheresAnimation(stage);
                case 1: Debug.Log("[AllAnimations] Return BalloonsAnimation"); return new BalloonsAnimation(stage);
                case 2: Debug.Log("[AllAnimations] Return BalloonsAtPixelsAnimation"); return new BalloonsAtPixelsAnimation(stage);
                default: Debug.Log("[AllAnimations] Return default SpheresAnimation"); return new SpheresAnimation(stage);
            }
        }
    }

    /// <summary>Port of PixelShaderSet.h / .cpp.</summary>
    public abstract class PixelShaderSet
    {
        protected SortedList<double, int> weightedSet = new SortedList<double, int>();
        protected double totalWeight = 0.0;

        protected void Add(int creationIndex, double weight)
        {
            weightedSet[weight + totalWeight] = creationIndex;
            totalWeight += weight;
        }

        protected abstract APixelShader CreateIndex(int index);

        public APixelShader Next()
        {
            if (weightedSet.Count == 0) return null;

            double choice = FireflyUtils.Rand1() * totalWeight;
            return CreateIndex(SetUtils.UpperBound(weightedSet, choice));
        }
    }

    public class AllPixelShaders : PixelShaderSet
    {
        public AllPixelShaders()
        {
            Add(0, 1.0); // NULL
            //Add(1, 1.0); // SparkleShader - fast, 90%, 50-100
            //Add(2, 1.0); // SparkleShader - middle speed, 100%, 0-100
            //Add(3, 1.0); // SparkleShader - middle speed, 10%, 100-250
        }

        protected override APixelShader CreateIndex(int index)
        {
            switch (index)
            {
                case 0:
                    Debug.Log("[AllPixelShaders] Return NULL");
                    return null;
                case 1:
                    Debug.Log("[AllPixelShaders] Return SparkleShader - fast, 90% 50-100");
                    return new SparkleShader(15, 15, 0.9, 1.0, 0.5);
                case 2:
                    Debug.Log("[AllPixelShaders] Return SparkleShader - middle speed, 100%, 0-100");
                    return new SparkleShader(50, 50, 1.0, 1.0, 0.0);
                case 3:
                    Debug.Log("[AllPixelShaders] Return SparkleShader - middle speed, 10%, 100-250");
                    return new SparkleShader(20, 50, 0.1, 2.5, 1.0);
                default:
                    Debug.Log("[AllPixelShaders] Return default NULL");
                    return null;
            }
        }
    }

    /// <summary>Port of ColorPaletteSet.h / .cpp.</summary>
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

    /// <summary>Port of ColorSchemeSet.h / .cpp.</summary>
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
