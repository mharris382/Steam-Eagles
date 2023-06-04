using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Buildables.Installers
{
    [InfoBox("Should be on GameObjectContext level within Building Context")]
    public class BuildablesBuildingInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            HypergasInstaller.Install(Container);
        }
    }

}