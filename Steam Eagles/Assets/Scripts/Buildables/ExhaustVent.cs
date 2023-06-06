using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace Buildables
{
    public class ExhaustVent : MonoBehaviour
    {
        [Required,ChildGameObjectsOnly] public MachineCell cell;
        public UnityEvent<float> onSteamConsumed;
        private ExhaustVentController _controller;


        [Inject]
        public void InjectMe(ExhaustVentController.Factory controllerFactory)
        {
            _controller = controllerFactory.Create(this);
        }
    }
}