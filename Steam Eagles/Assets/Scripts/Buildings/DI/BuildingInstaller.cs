using UnityEngine;
using Zenject;

namespace Buildings.DI
{
    [RequireComponent(typeof(Building))]
    public class BuildingInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            //Container.Rebind<Building>().FromComponentOn(gameObject).AsSingle();
            
        }
    }

}