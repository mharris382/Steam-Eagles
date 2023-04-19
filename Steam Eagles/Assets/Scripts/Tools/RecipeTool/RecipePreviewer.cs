using System;
using Buildables;
using Buildings;
using Cysharp.Threading.Tasks;
using Items;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Tools.RecipeTool
{
    public class RecipePreviewer : IDisposable
    {
        public SpriteRenderer previewSprite;
        public BuildableMachineBase previewMachine;
        private BuildableMachineBase _machinePrefab;
        
        
        private IDisposable _releasePreview;
        private readonly PreviewConfig _config;
        public bool isReady { get; private set; }
        public RecipePreviewer(MonoBehaviour caller, PreviewConfig config, Recipe recipe)
        {
            this._config = config;
            Debug.Assert(recipe != null);
            this.isReady = false;
            previewSprite = config.GetPreviewSprite();
            caller.StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                var machinePrefabLoadOp = recipe.InstanceReference.LoadAssetAsync<GameObject>();
                await machinePrefabLoadOp;
                var machinePrefab = machinePrefabLoadOp.Result;
                Debug.Assert(machinePrefabLoadOp.Status == AsyncOperationStatus.Succeeded, $"Failed to load machine for recipe: {recipe.name}", caller);
                _machinePrefab = machinePrefab.GetComponent<BuildableMachineBase>();
                _machinePrefab.CopySizeOntoPreviewSprite(previewSprite);
                previewMachine = machinePrefabLoadOp.Result.GetComponent<BuildableMachineBase>();
                isReady = true;
            }));
        }

        public void UpdatePreview(Building building, Vector3 selectedPositionWorld)
        {
            if (!isReady)
            {
                Debug.Log("Loading prefab for preview");
                return;
            }
            //machines should be placed on the same grid as the solid building layer
            var cell = building.Map.WorldToCell(selectedPositionWorld, BuildingLayers.SOLID);
            cell = FindBestCell(building, cell);

            var position = building.Map.CellToWorld(cell, BuildingLayers.SOLID);
            previewSprite.transform.position = position;
            _machinePrefab.CopySizeOntoPreviewSprite(previewSprite);
            previewSprite.color = _machinePrefab.IsPlacementValid(building, cell) ? _config.validColor : _config.invalidColor;
        }

        private static Vector3Int FindBestCell(Building building, Vector3Int cell)
        {
            const int solidTileCheck = 5;
            var current = cell;
            var tile = building.Map.GetTile(current, BuildingLayers.SOLID);
            int checks = 0;
            //try to find a solid tile below the selected position
            for (int i = 0; i < solidTileCheck; i++)
            {
                tile = building.Map.GetTile(current + Vector3Int.down, BuildingLayers.SOLID);
                if (tile != null)
                {
                    cell = current;
                }
                else
                {
                    current += Vector3Int.down;
                }
            }

            return cell;
        }

        public void CreatePreview(Recipe recipe)
        {
            Debug.LogWarning($"{nameof(RecipePreviewer)} throw new System.NotImplementedException();");
        }

        public void Dispose()
        {
        }
    }
}