using UnityEngine;
using Zenject;

namespace Weather.Storms
{
    
    public class StormSystemsInstaller : Installer<StormSystemsInstaller>
    {
        
        
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<StormTester>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<StormManager>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<StormSubjectManager>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PCStormSubjectManager>().AsSingle().NonLazy();

            Container.BindFactory<Bounds, Vector2, Vector2, Storm, Storm.Factory>().AsSingle().NonLazy();
            //  Container.QueueForInject(stormConfig);
        }
    }


    public interface IGlobalStormSystem
    {
        
    }
}