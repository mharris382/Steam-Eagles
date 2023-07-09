using System;
using CoreLib.SharedVariables;
using Items;
using Sirenix.OdinInspector;
using UI.Crafting;
using UI.Crafting.Destruction;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;
using UniRx;

public class UICraftingInstaller : MonoInstaller
{
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
        
        
        Container.Bind<HoverPosition>().FromInstance(hoverPosition).AsSingle();
    }


    Recipes GetRecipes(InjectContext context)
    {
        var uiCrafting = context.Container.Resolve<UICrafting>();
        return uiCrafting.recipes;
    }
}

[Serializable]
public class HoverPosition
{
    
     public Transform hoverTransform;
    public SharedTransform hoverPositionShared;


    [Inject]
    public void Init(UICrafting uiCrafting, CraftingAimHanding craftingAimHanding)
    {
        
        if (hoverTransform == null)
        {
            hoverTransform = new GameObject($"Aim Hover Point ({uiCrafting.name})").transform;
        }
        craftingAimHanding.AimWorldSpace.Subscribe(UpdateAimPosition);
        Debug.Assert(hoverPositionShared != null);
        hoverPositionShared.Value = hoverTransform;
    }

    void UpdateAimPosition(Vector3 vector3)
    {
        hoverTransform.position = vector3;
    }
}