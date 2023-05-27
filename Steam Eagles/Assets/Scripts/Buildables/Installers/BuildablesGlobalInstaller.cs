using Buildings;
using CoreLib;
using Sirenix.OdinInspector;
using Zenject;

namespace Buildables.Installers
{
    [InfoBox("Should be bound at the Project Context level")]
    public class BuildablesGlobalInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<BuildablesRegistry>().AsSingle().NonLazy();
        }
    }
}