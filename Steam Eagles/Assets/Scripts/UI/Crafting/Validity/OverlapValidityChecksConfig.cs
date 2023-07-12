using System;
using Buildables;
using Buildings;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class OverlapValidityChecksConfig
{
    [EnumToggleButtons]
    public BuildingLayers checkOverlapsForLayers =
        BuildingLayers.SOLID | BuildingLayers.SOLID;

    public LayerMask contactFilterForLayers;

    public bool CheckOverlapForLayer(BuildingLayers layers)
    {
            
        return (checkOverlapsForLayers & layers) != 0;
    }

    public bool CheckOverlapForMachine(BuildableMachineBase machineBase)
    {
        return false;
    }

    public ContactFilter2D GetContactFilterForOverlaps()
    {
        var filter = new ContactFilter2D();
        filter.SetLayerMask(contactFilterForLayers);
        filter.useLayerMask = true;
        filter.useTriggers = false;
        filter.useDepth = false;
        return filter;
    }
}