using CoreLib.Entities;
using Items;
using Zenject;

namespace Tools.BuildTool
{
    public class CharacterToolsInstaller : MonoInstaller
    {
        public ToolBeltInventory toolBeltInventory;
        public EntityInitializer entityInitializer;
        public CharacterTools characterTools;
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<ToolBeltToCharacterTools>().AsSingle().NonLazy();
            Container.Bind<EntityInitializer>().FromInstance(entityInitializer).AsSingle().NonLazy();
            Container.Bind<ToolBeltInventory>().FromInstance(toolBeltInventory).AsSingle().NonLazy();
            Container.Bind<CharacterTools>().FromInstance(characterTools).AsSingle().NonLazy();
        }
    }
}