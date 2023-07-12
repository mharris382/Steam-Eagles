using System;
using CoreLib.SharedVariables;
using CoreLib.Structures;
using Items;
using Sirenix.OdinInspector;
using UI.Crafting;
using UI.Crafting.Destruction;
using UI.Crafting.Events;
using UI.Crafting.Sampling;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;
using UniRx;
using UnityEngine.Serialization;

[Serializable]
public class CraftingConfig
{
    [FormerlySerializedAs("overlapValidityChecks")]
    public OverlapValidityChecksConfig overlapValidityChecksConfig;
}

public class HoverPositionBinder : IInitializable, IDisposable
{
    public HoverPosition hoverPosition;
    private readonly CraftingAimHanding _aimHanding;
    private CompositeDisposable _disposable = new();

    public HoverPositionBinder(HoverPosition hoverPosition, CraftingAimHanding aimHanding)
    {
        this.hoverPosition = hoverPosition;
        _aimHanding = aimHanding;
        if (this.hoverPosition.hoverTransform == null)
            this.hoverPosition.hoverTransform =
                new GameObject($"Hover Transform: {hoverPosition.hoverPositionShared.name}").transform;
    }

    public void Initialize()
    {
        _aimHanding.AimWorldSpace.Subscribe(t => this.hoverPosition.hoverTransform.position = t).AddTo(_disposable);
        ;
    }

    public void Dispose()
    {
        _disposable?.Dispose();
    }
}

public class UICraftingInstaller : MonoInstaller
{
    public CraftingConfig config;
    public CraftingSampleConfig sampleConfig;
    public HoverPosition hoverPosition;

    public override void InstallBindings()
    {
        Container.Bind<UICrafting>().FromComponentOn(gameObject).AsSingle().NonLazy();
        Container.Bind<Recipes>().FromMethod(GetRecipes).AsSingle();
        Container.Bind<LoadHelper>().AsSingle().NonLazy();
        Container.Bind<PlacementValidity>().AsSingle();
        Container.Bind<RecipePreviewController>().AsSingle();
        Container.Bind<DestructionPreviewController>().AsSingle();
        Container.Bind<CraftingAimHanding>().AsSingle().NonLazy();
        Container.BindFactory<Recipe, TileBase, TilePreview, TilePreview.Factory>().AsSingle();
        Container.BindFactory<Recipe, GameObject, PrefabPreview, PrefabPreview.Factory>().AsSingle();
        Container.Bind<PrefabPreviewCache>().AsSingle().NonLazy();

        Container.Bind<DestructionPreview>().AsSingle().NonLazy();
        Container.BindFactory<Recipe, TileDestructionHandler, TileDestructionHandler.Factory>().AsSingle();
        Container.BindFactory<Recipe, MachineDestructionHandler, MachineDestructionHandler.Factory>().AsSingle();
        Container.Bind<DestructionHandlers>().AsSingle().NonLazy();


        Container.Bind<CraftingBuildingTarget>().AsSingle().NonLazy();

        Container.Bind<HoverPosition>().FromInstance(hoverPosition).AsSingle();
        Container.Bind<CraftingDirectionHandler>().AsSingle().NonLazy();
        Container.Bind<TilePlacementValidityChecks>().AsSingle();
        Container.Bind<PrefabPlacementValidityChecks>().AsSingle();
        Container.Bind<OverlapValidityChecksConfig>().FromInstance(config.overlapValidityChecksConfig).AsSingle()
            .NonLazy();
        Container.Bind<CraftingConfig>().FromInstance(config).AsSingle();


        Container.Bind<CraftingEventPublisher>().AsSingle();
        Container.BindInterfacesAndSelfTo<HoverPositionBinder>().AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<CraftingSampler>().AsSingle().NonLazy();
        Container.Bind<CraftingSampleConfig>().FromInstance(sampleConfig).AsSingle();
        Container.Bind<ISampler>().To<MachineSampler>().AsCached();
        Container.Bind<ISampler>().To<TileSampler>().AsCached();
        Container.Bind<TileSampler.LadderTileSampler>().AsSingle();
        Container.Bind<TileSampler.WireTileSampler>().AsSingle();
        Container.Bind<TileSampler.WallTileSampler>().AsSingle();
        Container.Bind<TileSampler.PipeTileSampler>().AsSingle();
        Container.Bind<TileSampler.SolidTileSampler>().AsSingle();
    }


    Recipes GetRecipes(InjectContext context)
    {
        var uiCrafting = context.Container.Resolve<UICrafting>();
        return uiCrafting.recipes;
    }
}

[Serializable]
public class HoverPosition : IDisposable
{
    public Transform hoverTransform;
    [Required]  public SharedTransform hoverPositionShared;
    private IDisposable _d;


    [Inject]
    public void Init(UICrafting uiCrafting, CraftingAimHanding craftingAimHanding)
    {
        if (hoverTransform == null)
        {
            hoverTransform = new GameObject($"Aim Hover Point ({uiCrafting.name})").transform;
        }
        uiCrafting.hoverPosition = this;
        _d = craftingAimHanding.AimWorldSpace.Subscribe(UpdateAimPosition);
        hoverPositionShared.Value = hoverTransform;
        Debug.Assert(hoverPositionShared != null);
    }

    void UpdateAimPosition(Vector3 vector3)
    {
        hoverTransform.position = vector3;
    }

    public void Dispose()
    {
        _d?.Dispose();
    }
}