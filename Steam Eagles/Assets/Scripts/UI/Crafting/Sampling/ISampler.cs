using System;
using Buildings;
using Items;
using UnityEngine;

namespace UI.Crafting.Sampling
{
    public interface ISampler
    {
        bool TryGetRecipe(Building building, Vector3 aimPosition, out Recipe recipe);
    }
    
    public class SampleOrderAttribute : Attribute
    {
        public SampleOrderAttribute(int order)
        {
            Order = order;
        }

        public   int Order { get; }
    }
}