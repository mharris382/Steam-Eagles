using Zenject;

namespace CoreLib.Audio
{
    public class GlobalAudioInstaller : MonoInstaller<GlobalAudioInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<FMODParameterDatabase>().FromScriptableObjectResource("Audio/ParameterDatabase").AsSingle();
            Container.BindInterfacesAndSelfTo<FMODTagDatabase>().FromScriptableObjectResource("Audio/FMODTagDatabase").AsSingle();
            Container.BindInterfacesAndSelfTo<FMODParameterDatabase.Linker>().AsSingle().NonLazy();
        }
    }
}