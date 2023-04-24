using System.Collections;
using Buildings;
using Buildings.Tiles;
using UnityEngine;

namespace Buildables.Parts
{
    public class PipeAttachmentPart : BuildableMachinePart
    {
        
        protected override BuildingLayers Layer => BuildingLayers.PIPE;

        public override void OnBuild(Building building)
        {
            var cell = GetCell(building);
            if (!building.Tiles.isReady)
            {
                StartCoroutine(WaitForTiles(building));
                return;
            }

            var pipeTile = building.Tiles.PipeTile as EditableTile;
            building.Map.SetTile(cell, pipeTile);
        }

        IEnumerator WaitForTiles(Building building)
        {
            while(!building.Tiles.isReady)
                yield return null;
            OnBuild(building);
        }
    }
}