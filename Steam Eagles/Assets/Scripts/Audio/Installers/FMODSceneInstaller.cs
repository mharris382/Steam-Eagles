using System;
using CoreLib.Audio;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

using FMODEvent = FMODUnity.EventReference;

[Serializable]
public class BlockEvents
{
    [Tooltip("Should play when an item is picked up.")]
    public FMODEvent tileBuildEvent;
    
    public string craftingTypeParameter = "CraftingActionType";
    
    [Tooltip("Should tile is built by a player.")]
    public FMODEvent blockPlaceEvent;
    
    [Tooltip("Should tile is deconstructed by a player.")]
    public FMODEvent blockBreakEvent;
}
public class FMODSceneInstaller : MonoInstaller
{
    [BoxGroup("Events")]public FMODEvent musicEvent;
    [BoxGroup("Events")]public FMODEvent ambianceEvent;
    
    [BoxGroup("Events")]public FMODEvent footstepEvent;
    
    [FoldoutGroup("Events/Crafting")] public FMODEvent tileCraftingEvent;
    [FoldoutGroup("Events/Crafting")] public FMODEvent prefabCraftingEvent;
    
    [HideInInspector]
    public BlockEvents blockEvents;
    
    [BoxGroup("Labeled Parameters")] public FMODLabeledParameter surfaceParameter;
    [BoxGroup("Labeled Parameters")] public FMODLabeledParameter craftingTypeParameter;
    
    public override void InstallBindings()
    {
        Container.Bind<FMODSceneLoadCallback>().FromComponentOn(gameObject).AsSingle().NonLazy();
        Container.Bind<FMODEvent>().WithId(FMODEventIDs.FOOTSTEP).FromInstance(footstepEvent).AsCached().NonLazy();
        Container.Bind<FMODEvent>().WithId(FMODEventIDs.MUSIC).FromInstance(musicEvent).AsCached().WhenInjectedInto<FMODMusicPlayer>().NonLazy();
        Container.Bind<FMODEvent>().WithId(FMODEventIDs.AMBIANCE).FromInstance(ambianceEvent).AsCached().NonLazy();
        Container.Bind<FMODEvent>().WithId(FMODEventIDs.TILE_ACTION).FromInstance(tileCraftingEvent).AsCached().NonLazy();
        Container.Bind<FMODEvent>().WithId(FMODEventIDs.CRAFTING_ACTION).FromInstance(prefabCraftingEvent).AsCached().NonLazy();
        
        Container.Bind<FMODLabeledParameter>().WithId(FMODLabeledParameterIDs.SURFACE).FromInstance(surfaceParameter).AsCached();
        Container.Bind<FMODLabeledParameter>().WithId(FMODLabeledParameterIDs.CRAFTING_TYPE).FromInstance(craftingTypeParameter).AsCached();
        
        Container.BindInterfacesAndSelfTo<FMODMusicPlayer>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<FMODAmbiancePlayer>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<FMODFootstepPlayer>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<FMODCraftingPlayer>().AsSingle().NonLazy();

        

    }
}

public static class FMODEventIDs
{
    public const string MUSIC = "music";
    public const string AMBIANCE = "ambiance";
    public const string FOOTSTEP = "footstep";
    public const string TILE_ACTION = "Tile Action";
    public const string CRAFTING_ACTION = "Crafting Action";
}

public static class FMODLabeledParameterIDs
{
    public const string SURFACE = "Surface";
    public const string CRAFTING_TYPE = "CraftingActionType";
}