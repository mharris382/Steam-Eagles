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
            _recipePickups = await UniTask.WhenAll(_recipe.components.Select(t => t.item)
                .Where(t => t != null)
                .Select(t => t.LoadPickup()));
        }

        public void SpawnFrom(DestructParams destructParams)
        {
                
            Debug.Log($"Spawning pickups for {_recipe.name} ");
            foreach (var recipePickup in _recipePickups)
            {
                if (recipePickup == null) continue;
                var instance =recipePickup.SpawnPickup(destructParams.position);
                MessageBroker.Default.Publish(new SpawnedPickupInfo(recipePickup, instance.gameObject));
            }
        }
    }
}