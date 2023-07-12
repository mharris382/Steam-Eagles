using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Buildings;
using Items;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Crafting.Sampling
{
    [Serializable]
    public class CraftingSampleConfig
    {
        public string sampleActionName = "Sample";
    }
    public class CraftingSampler
    {
        public List<ISampler> Samplers { get; }
        private readonly CraftingAimHanding _aimHanding;
        private readonly CraftingSampleConfig _config;

        public CraftingSampler(CraftingAimHanding aimHanding, List<ISampler> samplers, CraftingSampleConfig config)
        {
            
            _aimHanding = aimHanding;
            _config = config;
            samplers = samplers.OrderBy(t =>
            {
                var order = t.GetType().GetAttribute<SampleOrderAttribute>();
                if (order == null) return 0;
                return order.Order;
            }).ToList();
            var sb = new StringBuilder();
            foreach (var sampler in samplers)
            {
                sb.AppendLine(sampler.GetType().Name);
            }
            Samplers = samplers;
            Debug.Log(sb.ToString());
        }


        public bool CheckForRecipeSampled(PlayerInput playerInput, Building targetBuilding, out Recipe sampledRecipe)
        {
            if (playerInput.actions[_config.sampleActionName].IsPressed())
            {
                foreach (var sampler in Samplers)
                {
                    if (sampler.TryGetRecipe(targetBuilding, _aimHanding.AimWorldSpace.Value, out sampledRecipe))
                    {
                        return true;
                    }
                }
            }
            sampledRecipe = null;
            return false;
        }
    }
}