using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Power
{
    [GlobalConfig("Resources/PowerNetworkConfigs")]
    public class PowerNetworkConfigs : GlobalConfig<PowerNetworkConfigs>
    {
        public SteamNetworkConfig steamNetworkConfig;
        
        [Serializable]
        public class ElectricityNetworkConfig
        {
            
        }
        
        [Serializable]
        public class SteamNetworkConfig
        {
            [Tooltip("How often the steam network should update it's state in seconds."), SuffixLabel("sec")]
            public float updateRate = 1f;
            
            [Tooltip("Maximum amount of pressure that can exist in a single network node."), SuffixLabel("kPa")]
            public float maxPressure = 100f;
            
            public float maxTemperature = 100f;
            
            public float minTemperature = 0f;
            
            public float pipeCapacity = 100f;
        }
    }
    
}