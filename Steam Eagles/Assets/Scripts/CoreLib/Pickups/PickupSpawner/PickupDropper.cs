using System;
using System.Linq;
using Buildables;
using Buildings;
using Buildings.Tiles;
using CoreLib.Structures;
using Cysharp.Threading.Tasks;
using Items;
using UniRx;
using UnityEngine;
using Zenject;

namespace CoreLib.Pickups.PickupSpawner
{
    public class PickupDropper : IInitializable, IDisposable
    {
        private readonly PickupCache _cache;
        private readonly CoroutineCaller _coroutineCaller;
        private CompositeDisposable _disposable;

        public PickupDropper(PickupCache cache, CoroutineCaller coroutineCaller)
        {
            _cache = cache;
            _coroutineCaller = coroutineCaller;
        }
        public void Initialize()
        {
            _disposable = new CompositeDisposable();
             MessageBroker.Default.Receive<TileEventInfo>()
                .Where(t =>
                    t.type == CraftingEventInfoType.SWAP ||
                    t.type == CraftingEventInfoType.DECONSTRUCT)
                .Subscribe(OnTileDeconstructed).AddTo(_disposable);
             MessageBroker.Default.Receive<PrefabEventInfo>()
                 .Where(t =>
                     t.type == CraftingEventInfoType.SWAP ||
                     t.type == CraftingEventInfoType.DECONSTRUCT)
                 .Subscribe(OnPrefabDeconstructed).AddTo(_disposable);
        }

        void OnPrefabDeconstructed(PrefabEventInfo info)
        {
            var buildableMachine = info.prefab.GetComponent<BuildableMachineBase>();
            if(buildableMachine == null) return;
            var spawnArea = buildableMachine.WsRect;
            var cells = buildableMachine.GetCells().Select(t =>
                info.building.GetComponent<Building>().Map.CellToWorldCentered(t, BuildingLayers.SOLID)).ToArray();
            var recipeInstance = info.prefab.GetComponent<RecipeInstance>();
            if (recipeInstance == null) return;
            _coroutineCaller.StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                var recipe = recipeInstance.Recipe;
                foreach (var recipeComponent in recipe.components)
                {
                    if(string.IsNullOrEmpty(recipeComponent.ItemName)) continue;
                    var pickup = await _cache.GetPickup(recipeComponent.ItemName);
                    for (int i = 0; i < recipeComponent.Count; i++)
                    {
                        var index = UnityEngine.Random.Range(0, cells.Length);
                        var pos = (Vector2)cells[index];
                        var offset = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(0, 1f);
                        pickup.SpawnPickup(pos + offset);
                    }
                }
            }));
        }
        void OnTileDeconstructed(TileEventInfo info)
        {
            var pos = (Vector2) info.wsPosition;
            var tile = info.oldTile as EditableTile;
            if (tile == null) return;
            _coroutineCaller.StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                var recipe = await tile.recipeName.LoadRecipe();
                foreach (var recipeComponent in recipe.components)
                {
                    if(string.IsNullOrEmpty(recipeComponent.ItemName)) continue;
                    var pickup = await _cache.GetPickup(recipeComponent.ItemName);
                    for (int i = 0; i < recipeComponent.Count; i++)
                    {
                        var offset = UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(0, 1f);
                        pickup.SpawnPickup(pos + offset);
                    }
                }
            }));
        }
        
        

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}