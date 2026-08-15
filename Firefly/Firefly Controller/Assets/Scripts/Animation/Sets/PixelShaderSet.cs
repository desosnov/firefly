using System.Collections.Generic;
using UnityEngine;

namespace Firefly
{
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
}
