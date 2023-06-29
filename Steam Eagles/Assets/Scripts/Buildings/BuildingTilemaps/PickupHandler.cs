using System.Collections.Generic;
using System.Linq;
using Buildings.Tiles;
using CoreLib;
using CoreLib.Interfaces;
using CoreLib.Pickups;
using Cysharp.Threading.Tasks;
using Items;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildings.BuildingTilemaps
{
    public interface IPickupHandlerFactory
    {
        
    }
    public class PickupHandler
    {
        public class Factory : PlaceholderFactory<Recipe, PickupHandler> { }
        
        private readonly MonoBehaviour _caller;
        private readonly EditableTile _tile;
        private readonly Recipe _recipe;
        private Pickup[] _recipePickups;
        private Dictionary<ItemBase, Pickup> _pickupLookup = new();
        private int[] _counts;
            
        public bool IsReady =>_recipePickups != null && _recipePickups.All(t => t != null);
        [Inject]
        public PickupHandler(CoroutineCaller caller, Recipe recipe) : this((MonoBehaviour)caller, recipe)
        {
        }
        public PickupHandler(MonoBehaviour caller, Recipe recipe)
        {
            _caller = caller;
            _recipe = recipe;
            Debug.Log("Loading pickups");
            caller.StartCoroutine(UniTask.ToCoroutine(async () => await LoadPickups()));
        }


        public async UniTask LoadPickups()
        {
            _counts = _recipe.components.Select(t => t.Count).ToArray();
            _recipePickups = await UniTask.WhenAll(_recipe.components.Select(t => t.item)
                .Where(t => t != null)
                .Select(t => t.LoadPickup()));
            for (int i = 0; i < _recipe.components.Count; i++)
            {
                if(_pickupLookup.ContainsKey(_recipe.components[i].item))
                    Debug.LogWarning($"Duplicate pickup for {_recipe.components[i].item.name}");
                else
                    _pickupLookup.Add(_recipe.components[i].item, _recipePickups[i]);
            }
        }
        public IEnumerable<PickupBody> SpawnFrom(DestructParams destructParams, ItemStack stack)
        {
            
            Debug.Log($"Spawning pickups for {_recipe.name} ");
            Debug.Assert(_pickupLookup.ContainsKey(stack.Item), $"No pickup found for item {stack.Item.name}");
            var pickup = _pickupLookup[stack.Item];
            var count = stack.Count;
            for (int j = 0; j < count; j++)
            {
                var instance= pickup.SpawnPickup(destructParams.position);
                MessageBroker.Default.Publish(new SpawnedPickupInfo(pickup, instance.gameObject));
                yield return instance;
            }
        }
        public void SpawnFrom(DestructParams destructParams)
        {
                
            Debug.Log($"Spawning pickups for {_recipe.name} ");
            for (int i = 0; i < _recipePickups.Length; i++)
            {
                var pickup = _recipePickups[i];
                var count = _counts[i];
                for (int j = 0; j < count; j++)
                {
                    var instance= pickup.SpawnPickup(destructParams.position);
                    MessageBroker.Default.Publish(new SpawnedPickupInfo(pickup, instance.gameObject));
                }
            }
        }
        public void SpawnLocal(Transform parent, Vector3 localPosition, DestructParams destructParams)
        {
                
            Debug.Log($"Spawning pickups for {_recipe.name} ");
            for (int i = 0; i < _recipePickups.Length; i++)
            {
                var pickup = _recipePickups[i];
                var count = _counts[i];
                for (int j = 0; j < count; j++)
                {
                    var instance= pickup.SpawnPickup(parent.TransformPoint(localPosition));
                    instance.transform.SetParent(parent, true);
                    var rb = instance.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.velocity =destructParams.direction * 2f  ;
                    }
                    MessageBroker.Default.Publish(new SpawnedPickupInfo(pickup, instance.gameObject));
                }
            }
        }
    }
}