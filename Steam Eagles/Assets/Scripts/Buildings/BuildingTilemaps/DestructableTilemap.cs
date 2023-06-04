using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Buildings.Tiles;
using CoreLib;
using CoreLib.Interfaces;
using Cysharp.Threading.Tasks;
using Items;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Buildings.BuildingTilemaps
{
    public abstract class DestructableTilemap : EditableTilemap, IDestruct
    {
        public virtual bool IsFullyDestroyed => false;
        public bool debug = true;
        public int checkIterations = 10;
        public bool TryToDestruct(DestructParams destructParams)
        {
            if (!IsFullyLoaded)
            {
                return false;
            }
            EditableTile tile = null;
            Vector3Int cellPosition = default;
            if (destructParams.isCellTarget)
            {
                cellPosition = destructParams.cellPosition;
                tile = Tilemap.GetTile(cellPosition) as EditableTile;
                if (tile == null)
                {
                    return false;
                }
                MessageBroker.Default.Publish(new DisconnectActionInfo(Tilemap, cellPosition, tile));
                Debug.Assert(_deconstructionPickupSpawners.ContainsKey(tile),
                    $"Destructable tile ({tile.name}) found on tilemap ({name}, Type:{this.GetType().Name}), but no pickup spawner found for it.", this);
                _deconstructionPickupSpawners[tile].SpawnLocal(transform, Tilemap.CellToLocal(cellPosition), new DestructParams(default, default));
                Building.Map.SetTile(cellPosition, this.Layer, null);
                //Tilemap.SetTile(cellPosition, null);
                return true;
            }
            else
            {
                var tilePosition = destructParams.position;/// + (destructParams.direction * 0.1f);
                cellPosition = Tilemap.layoutGrid.WorldToCell(tilePosition);
                var aboveCell = cellPosition + Vector3Int.up;
                var belowCell = cellPosition + Vector3Int.down;
                var leftCell = cellPosition + Vector3Int.left;
                var rightCell = cellPosition + Vector3Int.right;
            
                //= Tilemap.GetTile<EditableTile>(cellPosition);
                var cells = new List<Vector3Int>{cellPosition, aboveCell, belowCell, leftCell, rightCell, leftCell + Vector3Int.up, rightCell + Vector3Int.up, leftCell +Vector3Int.down, rightCell + Vector3Int.down};
                foreach (var cell in cells)
                {
                    tile = Tilemap.GetTile<EditableTile>(cell);
                    if (tile != null)
                    {
                        cellPosition = cell;
                        break;
                    }
                }
                if (tile == null)
                {
                    return false;
                }
                MessageBroker.Default.Publish(new DisconnectActionInfo(Tilemap, cellPosition, tile));
                Debug.Assert(_deconstructionPickupSpawners.ContainsKey(tile),
                    $"Destructable tile ({tile.name}) found on tilemap ({name}, Type:{this.GetType().Name}), but no pickup spawner found for it.", this);
                _deconstructionPickupSpawners[tile].SpawnFrom(destructParams);
                Tilemap.SetTile(cellPosition, null);
                return true;
            }
        }


        private Dictionary<EditableTile, PickupHandler> _deconstructionPickupSpawners = new Dictionary<EditableTile, PickupHandler>();

        public abstract IEnumerable<string> GetTileAddresses();

        void Awake() => IsFullyLoaded = false;
        protected bool IsFullyLoaded { get; private set; }
        
        public IEnumerator Start()
        {
            var tileAddresses = GetTileAddresses().ToArray();
            yield return UniTask.ToCoroutine(async () =>
            {
                var results = await UniTask.WhenAll(tileAddresses.Select(t =>
                        Addressables.LoadAssetAsync<EditableTile>(t).ToUniTask()));
                var recipes = await UniTask.WhenAll(results.Select(t => t.recipeName.LoadRecipe()));
                for (int i = 0; i < results.Length; i++)
                {
                    var tile = results[i];
                    var recipe = recipes[i];
                    Debug.Assert(recipe != null, $"Failed to load recipe {tile.recipeName} for tile {tile.name}",this);
                    _deconstructionPickupSpawners.Add(tile, recipe == null ? null : new PickupHandler(this, recipe));
                }
                await UniTask.WhenAll(_deconstructionPickupSpawners.Values.Select(t => t.LoadPickups()));
                Debug.Log($"Fully loaded pickups for tilemap: {name}",this);
                IsFullyLoaded = true;
            });
         
        }

    
    }
}