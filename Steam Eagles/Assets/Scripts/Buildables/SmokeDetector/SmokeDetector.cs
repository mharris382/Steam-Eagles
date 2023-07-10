using Buildables.Interfaces;
using Buildables.Utilities;
using Buildings;
using UnityEngine;

namespace Buildables.SmokeDetector
{
    public class SmokeDetector : TileUtility
    {
        public override BuildingLayers TargetLayer => BuildingLayers.SOLID;
        public override void OnMachineBuilt(BuildableMachineBase machineBase)
        {
            
        }

        public override void OnMachineRemoved(BuildableMachineBase machineBase)
        {
            
        }
    }
}