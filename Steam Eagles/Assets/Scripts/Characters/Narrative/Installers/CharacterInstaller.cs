using Zenject;

namespace Characters.Narrative.Installers
{
    public class CharacterInstaller : MonoInstaller
    {
        public CharacterStaminaConfig characterStaminaConfig;
        public override void InstallBindings()
        {
            Container.Bind<CharacterStaminaConfig>().FromInstance(characterStaminaConfig).AsSingle();
            CharacterSprintInstaller.Install(Container);
        }
    }
}