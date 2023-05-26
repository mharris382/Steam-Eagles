using Buildings;
using CoreLib;
using CoreLib.Signals;
using UnityEngine;
using Zenject;

namespace Weather.Storms
{

    public class StormSystemsInstaller : Installer<StormSystemsInstaller>
    {


        public override void InstallBindings()
        {


            Container.BindInterfacesAndSelfTo<StormManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<BuildingStormSubjectManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PcStormSubjectManager>().AsSingle().NonLazy();

            Container.BindFactory<IStormSubject, StormSubject, StormSubject.Factory>().AsSingle();
            Container.BindFactory<PCInstance, IPCTracker, GameObject, Camera, PCStormSubject, PCStormSubject.Factory>().AsSingle();
            Container.BindFactory<Building, BuildingStormSubject, BuildingStormSubject.Factory>().AsSingle();
            Container.BindFactory<Bounds, Vector2, Vector2, Storm, Storm.Factory>().AsSingle();

            Container.BindInterfacesAndSelfTo<StormTester>().AsSingle().NonLazy();
            //  Container.QueueForInject(stormConfig);
        }
    }
}