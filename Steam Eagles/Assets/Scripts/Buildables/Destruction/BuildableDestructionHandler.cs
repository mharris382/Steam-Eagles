using System;
using System.Collections.Generic;
using System.Linq;
using Buildings.BuildingTilemaps;
using CoreLib.Interfaces;
using Items;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildables
{
    [Serializable]
    public class DestructionDropperConfig
    {
        [SerializeField] private DropStrategy dropStrategy;
        [SerializeField] private int dropQuantity = 10;
        
        public int DropQuantity => dropQuantity;
        
        private enum DropStrategy
        {
            QUANTITY_DESCENDING,
            RECIPE_ORDER
        }
    }
    public interface IDropperFactoryAdvanced : IFactory<Recipe,DestructionDropperConfig, IDestructionResourceDropper>{}
    public interface IDropperFactorySimple : IFactory<Recipe, IDestructionResourceDropper> { }
    
    
    public interface IDestructionResourceDropper
    {
        int HitsTotal { get; }
        int HitsRemaining { get; }
        IEnumerable<ItemStack> DeconstructObject(int damage);
    }

    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(BuildableMachineBase))]
    public class BuildableDestructionHandler : MonoBehaviour, IDestruct
    {
        [Required] public Recipe recipe;
        public bool useHitBuffer = false;
        public float hitBufferRate = .1f;


        private Health _health;
        private BuildableMachineBase _machineBase;
        
        
        private Subject<DestructParams> _onMachineHit = new();
        private IDestructionResourceDropper _dropper;
        private PickupHandler _pickupHandler;
        private ReadOnlyReactiveProperty<int> _hitsRemaining;
        
        private Health Health => _health ??= GetComponent<Health>();
        
        
        [ShowInInspector, ReadOnly, HideInEditorMode]
        public bool IsFullyDestroyed =>  Health.IsDead;

        [ShowInInspector, ReadOnly, HideInEditorMode]
        public int HitsRemaining
        {
            get => _hitsRemaining != null ? _hitsRemaining.Value : -1;
            set {}
        }
        
        
        [Inject] private void InjectMe(
            IDropperFactorySimple dropperFactory, IDropperFactoryAdvanced dropperFactoryAdvanced, PickupHandler.Factory pickupHandlerFactory)
        {
            _dropper = dropperFactory.Create(recipe);
            _pickupHandler = pickupHandlerFactory.Create(recipe);
        }
  
        private void Awake()
        {
            _health = GetComponent<Health>();
            _machineBase = GetComponent<BuildableMachineBase>();
            
           
        }

        private void Start()
        {
            var hitStream = useHitBuffer
                ? _onMachineHit.Buffer(TimeSpan.FromSeconds(hitBufferRate)).Where(t => t.Count > 0).Select(t => t[0])
                : _onMachineHit;
            // var hitDropStream = hitStream.Select(t => _dropper.DeconstructObject(1));

            var hitResourceDropStream = hitStream.Select(t => (t, _dropper.DeconstructObject(1)))
                .SelectMany(t => t.Item2.Select(drop => (t.t, drop)))
                .Where(t => t.drop.Count > 0);
            _hitsRemaining = hitResourceDropStream.Select(_ => _dropper.HitsRemaining).ToReadOnlyReactiveProperty();
            hitResourceDropStream.Subscribe(tup =>
            {
                foreach (var pickup in _pickupHandler.SpawnFrom(tup.t, tup.drop))
                {
                    Debug.Log($"Spawning pickup: {pickup.name}");
                }
            }).AddTo(this);
        }

        public bool TryToDestruct(DestructParams destructParams)
        {
            _onMachineHit.OnNext(destructParams);
            return !IsFullyDestroyed;
        }


        private void SpawnItem(DestructParams destructParams, ItemStack stack)
        {
            
        }
        private void DestroyBuildable()
        {
            
        }
    }
}