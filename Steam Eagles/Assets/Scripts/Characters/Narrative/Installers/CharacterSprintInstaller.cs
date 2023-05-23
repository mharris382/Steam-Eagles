﻿using Zenject;

namespace Characters.Narrative.Installers
{
    public class CharacterSprintInstaller : Installer<CharacterSprintInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<CharacterSprintStaminaHandler>().AsSingle().NonLazy();
            Container.Bind<CharacterEntities>().AsSingle().NonLazy();
        }
    }
}