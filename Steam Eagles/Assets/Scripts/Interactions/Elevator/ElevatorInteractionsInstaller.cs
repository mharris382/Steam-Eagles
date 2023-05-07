using UnityEngine;
using Zenject;

namespace Interactions
{
    public class ElevatorInteractionsInstaller : MonoInstaller
    {
        
        public GameObject controller;
        public override void InstallBindings()
        {
            Container.Bind<ElevatorController>().FromComponentOn(controller).AsSingle().NonLazy();
            Container.BindFactory<InteractionAgent, ElevatorInteractable.InteractionHandle, ElevatorInteractable.InteractionHandle.Factory>();
            Container.BindFactory<InteractionAgent, InteractionOptionHandler, InteractionOptionHandler.Factory>();
            Container.BindInterfacesAndSelfTo<ElevatorPassengers>().AsSingle().NonLazy();
        }
    }
}