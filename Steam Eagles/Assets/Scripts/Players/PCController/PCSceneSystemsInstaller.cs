﻿using Players.PCController.Interactions;
using Zenject;

public class PCSceneSystemsInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        PCParallaxSystemsInstaller.Install(Container);
        PCInteractionSystemsInstaller.Install(Container);
    }
}