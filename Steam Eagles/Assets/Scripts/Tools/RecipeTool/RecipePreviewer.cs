using System;
using System.Collections.Generic;
using System.Linq;
using Buildables;
using Buildings;
using Buildings.Tiles;
using Cysharp.Threading.Tasks;
using Items;
using UniRx;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Tools.RecipeTool
{
    [System.Obsolete("Use LoadedRecipePreviewer instead")]
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
            switch (recipe.GetRecipeType())
            {
                case Recipe.RecipeType.TILE:
                    caller.StartCoroutine(UniTask.ToCoroutine(async () =>
                    {
                        var tileAsset = recipe.InstanceReference.LoadAssetAsync<EditableTile>();
                        isReady = true;
                    }));
                    break;
                case Recipe.RecipeType.MACHINE:
                    var loader = recipe.GetPrefabLoader(caller);
                    if (loader.IsLoaded)
                    {
                        isReady = true;
                    }
                    caller.StartCoroutine(UniTask.ToCoroutine(async () =>
                    {
                        var machinePrefabLoadOp = recipe.InstanceReference.LoadAssetAsync<GameObject>();
                        await machinePrefabLoadOp.ToUniTask();
                        var machinePrefab = machinePrefabLoadOp.Result;
                        _releasePreview = Disposable.Create(() => recipe.InstanceReference.ReleaseAsset());
                        Debug.Assert(machinePrefabLoadOp.Status == AsyncOperationStatus.Succeeded, $"Failed to load machine for recipe: {recipe.name}", caller);
                        _machinePrefab = machinePrefab.GetComponent<BuildableMachineBase>();
                        _machinePrefab.CopyOntoPreviewSprite(previewSprite);
                        previewMachine = machinePrefabLoadOp.Result.GetComponent<BuildableMachineBase>();
                        isReady = true;
                    }));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        Vector3Int _lastCell;
        public void UpdatePreview(Building building, Vector3 selectedPositionWorld, out bool positionValid, bool flipped = false)
        {
            if (!isReady)
            {
                Debug.Log("Loading prefab for preview");
                positionValid = false;
                return;
            }

            previewMachine.IsFlipped = flipped;
            
            //machines should be placed on the same grid as the solid building layer
            var cell = building.Map.WorldToCell(selectedPositionWorld, BuildingLayers.SOLID);
            cell = FindBestCell(building, cell);
            _lastCell = cell;

            var position = building.Map.CellToWorld(cell, BuildingLayers.SOLID);
            previewSprite.transform.position = position;
            _machinePrefab.CopyOntoPreviewSprite(previewSprite);
            // ReSharper disable once AssignmentInConditionalExpression
            previewSprite.color = (positionValid = _machinePrefab.IsPlacementValid(building, cell))
                ? _config.validColor
                : _config.invalidColor;
        }

        private  Vector3Int FindBestCell(Building building, Vector3Int cell)
        {
            if (previewMachine.snapsToGround == false)
            {
                return cell;
            }
            const int solidTileCheck = 3;
            
            var current = cell;
            var current2 = cell;
            var tile = building.Map.GetTile(current, BuildingLayers.SOLID);
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
                if (previewMachine.IsPlacementValid(building, pos))
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
            
            //try to find a solid tile below the selected position
            for (int i = 0; i < solidTileCheck; i++)
            {
                tile = building.Map.GetTile(current + Vector3Int.down, BuildingLayers.SOLID);
                if (tile == null) tile = building.Map.GetTile(current + Vector3Int.down, BuildingLayers.FOUNDATION);
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
            _releasePreview?.Dispose();
        }

        public BuildableMachineBase Build(Building b)
        {
            if (!isReady)
            {
                return null;
            }
            return previewMachine.Build(_lastCell, b);
        }

        public void SetVisible(bool visible)
        {
            if(previewSprite != null)
                previewSprite.gameObject.SetActive(visible);
        }
    }
}