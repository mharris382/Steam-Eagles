using UnityEngine;
using Zenject;

namespace Interactions
{
    public class ElevatorInteractionsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ElevatorController>().FromComponentOn(gameObject).AsSingle().NonLazy();
            Container.BindFactory<InteractionAgent, ElevatorInteractable.InteractionHandle,
                    ElevatorInteractable.InteractionHandle.Factory>();
        }
    }
}