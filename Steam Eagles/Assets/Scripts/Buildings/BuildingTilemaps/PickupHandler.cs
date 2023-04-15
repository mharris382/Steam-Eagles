using System.Linq;
using Buildings.Tiles;
using CoreLib;
using CoreLib.Interfaces;
using CoreLib.Pickups;
using Cysharp.Threading.Tasks;
using Items;
using UniRx;
using UnityEngine;

namespace Buildings.BuildingTilemaps
{
    public class PickupHandler
    {
        private readonly MonoBehaviour _caller;
        private readonly EditableTile _tile;
        private readonly Recipe _recipe;
        private Pickup[] _recipePickups;
        private int[] _counts;
            
        public bool IsReady =>_recipePickups != null && _recipePickups.All(t => t != null);

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