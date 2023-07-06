using System;
using Buildings;
using Buildings.Rooms;
using CoreLib;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;
using Zenject;
[RequireComponent(typeof(GasTexture))]
[RequireComponent(typeof(RoomEffect), typeof(RoomTextures), typeof(RoomSimTextures))]
[RequireComponent(typeof(Room))]
public class RoomGasSimInstaller : MonoInstaller
{
    [Serializable]
    public class GasResources
    {
        public VisualEffect visualEffect;
        public string gasTextureParameterName = "GasTexture";
       
        
        // public VisualEffect CreateEffectFor(Room room, RenderTexture gasTexture)
        // {
        //     if (!asPrefab) throw new NotImplementedException();
        //     var effect = Instantiate(visualEffect, room.transform);
        //     effect.SetTexture(gasTextureParameterName, gasTexture);
        //     return effect;
        // }
    }

    public GasResources gasResources;
    public override void InstallBindings()
    {
        Container.Rebind<DiContainer>().FromInstance(Container).AsSingle().NonLazy();
        
        Container.Rebind<Room>().FromComponentOn(gameObject).AsSingle().NonLazy();
        Container.Rebind<RoomTextures>().FromComponentOn(gameObject).AsSingle();
        
        Container.Bind<GasResources>().FromInstance(gasResources).AsSingle().NonLazy();
        Container.Bind<RoomEffect>().FromComponentOn(gameObject).AsSingle();
        Container.Bind<RoomSimTextures>().FromComponentOn(gameObject).AsSingle();

        Container.Bind<SimGrid>().FromNewComponentOnNewGameObject().WithGameObjectName("Sim Grid").UnderTransform(GetSimGridTransform).AsSingle();
    }

    Transform GetSimGridTransform(InjectContext context)
    {
        try
        {
            var building = context.Container.Resolve<Building>();
            return building.TilemapParent;
        }
        catch (Exception e)
        {
            Debug.LogError("Cannot find building in context.", this);
            throw;
        }
    }
}


public class GasSimTest : MonoBehaviour
{
    public VisualEffect visualEffect;
    public SimGrid simGrid;
    public RectInt rect = new RectInt(10, 10, 10, 10);

    bool HasRequirements => visualEffect && simGrid;
    private void OnDrawGizmos()
    {
        rect.DrawGizmos(simGrid.Grid);
    }
}