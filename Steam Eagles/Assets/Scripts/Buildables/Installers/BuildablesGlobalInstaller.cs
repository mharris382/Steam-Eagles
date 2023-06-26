using System;
using System.Collections.Generic;
using Buildings;
using Buildings.BuildingTilemaps;
using CoreLib;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Buildables.Installers
{
  public  class SimpleDropperFactory : PlaceholderFactory<Recipe, IDestructionResourceDropper>, IDropperFactorySimple {}
  public  class AdvancedDropperFactory : PlaceholderFactory<Recipe, DestructionDropperConfig, IDestructionResourceDropper>, IDropperFactoryAdvanced { }
    [InfoBox("Should be bound at the Project Context level")]
    public class BuildablesGlobalInstaller : MonoInstaller
    {
        [SerializeField] private DestructionDropperConfig defaultDropperConfig;
        [SerializeField] private HypergasEngineConfig hypergasEngineConfig;
       
        public override void InstallBindings()
        {
            Container.Bind<BuildablesRegistry>().AsSingle().NonLazy();
            Container.Bind<HypergasEngineConfig>().FromInstance(hypergasEngineConfig).AsSingle();
            Container.Bind<DestructionDropperConfig>().FromInstance(defaultDropperConfig).AsCached().WhenInjectedInto<SimpleDropperFactoryImp>();

            Container.Bind<IDropperFactorySimple>().To<SimpleDropperFactoryImp>().AsSingle().NonLazy();
            Container.Bind<IDropperFactoryAdvanced>().To<AdvancedDropperFactoryImp>().AsSingle().NonLazy();
          // Container.BindFactory<Recipe, IDestructionResourceDropper, SimpleDropperFactoryImp>().AsSingle().NonLazy();
          // Container.BindFactory<Recipe, DestructionDropperConfig, IDestructionResourceDropper, AdvancedDropperFactoryImp>();
            Container.BindFactory<Recipe, DestructionDropperConfig, DropperImp, DropperImp.Factory>().AsSingle().NonLazy();
            Container.BindFactory<Recipe, PickupHandler, PickupHandler.Factory>().AsSingle().NonLazy();

        }



        class AdvancedDropperFactoryImp :  IDropperFactoryAdvanced
        {
            private readonly DropperImp.Factory _dropperImpFactory;

            public AdvancedDropperFactoryImp(DropperImp.Factory dropperImpFactory)
            {
                _dropperImpFactory = dropperImpFactory;
            }
            public IDestructionResourceDropper Create(Recipe param1, DestructionDropperConfig original)
            {
                return _dropperImpFactory.Create(param1, original);
            }
        }
        
        class SimpleDropperFactoryImp : IDropperFactorySimple
        {
            private readonly DestructionDropperConfig _config;
            private readonly DropperImp.Factory _dropperImpFactory;

            public SimpleDropperFactoryImp(DestructionDropperConfig config, DropperImp.Factory dropperImpFactory)
            {
                _config = config;
                
                _dropperImpFactory = dropperImpFactory;
            }

            public IDestructionResourceDropper Create(Recipe param1)
            {
                return _dropperImpFactory.Create(param1, _config);
            }

            
        }

        class DropperImp : IDestructionResourceDropper
        {
            public class Factory : PlaceholderFactory<Recipe, DestructionDropperConfig, DropperImp>{}
            private Queue<ItemStack> _dropStacks = new();
            private readonly int _hitsTotal;

            public DropperImp(Recipe recipe, DestructionDropperConfig config)
            {
                _hitsTotal = 0;
                List<ItemStack> stacks = new();
                int dropQuantity = config.DropQuantity;
                int total = 0;
                foreach (var stack in recipe.components)
                {
                    int count = stack.Count;
                    total += count;
                    var item = stack.Item;
                    while (count > 0)
                    {
                        int toDrop = Mathf.Min(count, dropQuantity);
                        count -= toDrop;
                        stacks.Add(new ItemStack(stack.Item, toDrop));
                    }
                }
            }

            public int HitsRemaining => _dropStacks.Count;

            public int HitsTotal => _hitsTotal;

            public IEnumerable<ItemStack> DeconstructObject(int damage)
            {
                while (damage > 0 && _dropStacks.Count > 0)
                {
                    var dropStack = _dropStacks.Dequeue();
                    yield return dropStack;
                    damage--;
                }
            }
        }
    }




}