using Buildings;
using UnityEngine;

namespace Buildables.Parts
{
    public class PipeAttachmentPart : BuildableMachinePart
    {
        
        protected override BuildingLayers Layer => BuildingLayers.PIPE;

        public override void OnBuild(Building building)
        {
            var cell = GetCell(building);
            
            throw new System.NotImplementedException();
        }
    }
}