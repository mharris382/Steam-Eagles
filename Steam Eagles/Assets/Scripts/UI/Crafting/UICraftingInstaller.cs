using Items;
using UI.Crafting;
using UI.Crafting.Destruction;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

public class UICraftingInstaller : MonoInstaller
{
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
    }


    Recipes GetRecipes(InjectContext context)
    {
        var uiCrafting = context.Container.Resolve<UICrafting>();
        return uiCrafting.recipes;
    }
}