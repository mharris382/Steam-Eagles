using System.Collections.Generic;
using System.Linq;
using Buildables;
using Buildings;
using UnityEngine;

namespace Tools.RecipeTool
{
    public class LoadedRecipePreviewer
    {
        public readonly SpriteRenderer previewSprite;
        public readonly BuildableMachineBase machine;
        private readonly PreviewConfig _config;

        public BuildingLayers TargetLayer => machine.GetTargetLayer();
        
        public LoadedRecipePreviewer(PreviewConfig config, GameObject loadedPrefab)
        {
            _config = config;
            previewSprite = config.GetPreviewSprite();
            machine = loadedPrefab.GetComponent<BuildableMachineBase>();
            
        }

        public void SetVisible(bool visible)
        {
            previewSprite.gameObject.SetActive(visible);
            if (visible)
            {
                machine.CopyOntoPreviewSprite(previewSprite);
            }
        }

        public void UpdatePreview(Building building, ref Vector3Int hoveredPositionValue, out bool isValid,
            ref string errorMessage, bool isFlipped)
        {
            machine.IsFlipped = isFlipped;
            var cell = FindBestCell(building, hoveredPositionValue);
            var position = building.Map.CellToWorld(cell, TargetLayer);
            previewSprite.transform.position = position;
            isValid = machine.IsPlacementValid(building, ref cell, ref errorMessage);
            hoveredPositionValue = cell;
            previewSprite.color = isValid ? _config.validColor : _config.invalidColor;
        }
        
        private  Vector3Int FindBestCell(Building building, Vector3Int cell)
        {
            if (machine.snapsToGround == false)
            {
                return cell;
            }
            const int solidTileCheck = 3;
            
            var current = cell;
            var current2 = cell;
            var tile = building.Map.GetTile(current, TargetLayer);
            int checks = 0;

            List<Vector3Int> allValidCells = new List<Vector3Int>();
            List<Vector3Int> cellsToCheck = new List<Vector3Int>();
            for (int i = 0; i < solidTileCheck; i++)
            {
                current = current + Vector3Int.down;
                current2 = current2 + Vector3Int.up;
                cellsToCheck.Add(current);
            }

            foreach (var pos in cellsToCheck)
            {
                if (machine.IsPlacementValid(building, pos))
                {
                    allValidCells.Add(pos);
                }
            }
            
            if(allValidCells.Count== 0)
            {
                return cell;
            }

            var cell1 = cell;
            return allValidCells.OrderBy(t => Vector3Int.Distance(t, cell1)).FirstOrDefault();
        }
    }
}