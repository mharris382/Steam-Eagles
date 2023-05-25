using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Weather.Storms.Views
{
    [InfoBox("This should be on the storm view prefab along with a gameobject context. It will be used to install from subcontainer resolve")]
    public class StormViewInstaller : MonoInstaller
    {
        
        public override void InstallBindings()
        {
            Container.Bind<StormView>().FromComponentOn(this.gameObject).AsSingle().NonLazy();
        }
    }
}