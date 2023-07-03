using System;
using Buildings;
using Buildings.Rooms;
using CoreLib;
using UnityEngine;
using UnityEngine.VFX;
using Zenject;

public class RoomGasSimInstaller : MonoInstaller
{
    

    public override void InstallBindings()
    {
        Container.Bind<SimGrid>().FromNewComponentOnNewGameObject().WithGameObjectName("Sim Grid")
            .UnderTransform(GetSimGridTransform).AsSingle();
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