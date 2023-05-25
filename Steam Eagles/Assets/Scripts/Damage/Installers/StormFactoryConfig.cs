using System;
using Damage.Core;
using UnityEngine;

[Obsolete("Use Weather.Storms.StormView instead")]
[CreateAssetMenu(menuName = "Steam Eagles/Storms/Storm Config", fileName = "new Storm Config", order = 0)]
public class StormFactoryConfig : ScriptableObject, IStormFactory
{
    
    public StormHandle CreateStormInstance(int maxIntensity, int minIntensity)
    {
        throw new System.NotImplementedException();
    }
    
    
}