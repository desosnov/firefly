using System.Collections.Generic;
using UnityEngine;

namespace Firefly
{
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
            Add(3, 2.0); // SpinnerAnimation
        }

        protected override AAnimation CreateIndex(int index)
        {
            switch (index)
            {
                case 0: Debug.Log("[AllAnimations] Return SpheresAnimation"); return new SpheresAnimation(stage);
                case 1: Debug.Log("[AllAnimations] Return BalloonsAnimation"); return new BalloonsAnimation(stage);
                case 2: Debug.Log("[AllAnimations] Return BalloonsAtPixelsAnimation"); return new BalloonsAtPixelsAnimation(stage);
                case 3: Debug.Log("[AllAnimations] Return SpinnerAnimation"); return new SpinnerAnimation(stage);
                default: Debug.Log("[AllAnimations] Return default SpheresAnimation"); return new SpheresAnimation(stage);
            }
        }
    }
}
