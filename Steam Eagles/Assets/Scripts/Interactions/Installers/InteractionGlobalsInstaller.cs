using System.Collections.Generic;
using CoreLib;
using Interactions.LookAt;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Zenject;

namespace Interactions.Installers
{
    [InfoBox("Must be bound in project context")]
    public class InteractionGlobalsInstaller : MonoInstaller
    {
        public InteractionGlobalConfig globalConfig;
        public override void InstallBindings()
        {
            Container.BindFactory<InteractionAgent, AvailableInteractions, AvailableInteractions.Factory>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<AgentRegistry>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<InteractableRegistry>().AsSingle().NonLazy();
            Container.Bind<InteractionGlobalConfig>().FromInstance(globalConfig).AsSingle().NonLazy();
            Container.Bind<PlayerVCamFactory>().AsSingle().NonLazy();
        }
    }
    public class PlayerVCamFactory : IFactory<InteractionAgent, GameObject, GameObject>
    {
        private readonly GlobalPCInfo _globalPcInfo;
        private readonly LayerMaskFactory _layerMaskFactory;

        public PlayerVCamFactory(GlobalPCInfo globalPcInfo)
        {
            _globalPcInfo = globalPcInfo;
            _layerMaskFactory = new LayerMaskFactory();
        }
        public  GameObject Create(InteractionAgent param1, GameObject param2)
        {
            int pcIndex = -1;
            for (int i = 0; i < _globalPcInfo.PCCount; i++)
            {
                var pc = _globalPcInfo.GetInstance(i);
                if (pc.character == param1.gameObject)
                {
                    pcIndex = i;
                    break;   
                }
            }

            if (pcIndex == -1)
                return null;
            var vCam = GameObject.Instantiate(param2, param2.transform.position, param2.transform.rotation, param2.transform.parent);
            vCam.layer = _layerMaskFactory.Create(pcIndex);
            return vCam;
        }
    }


    /// <summary>
    /// keeps track of all agents and updates their available interactions
    /// GLOBAL SINGLETON (bound in Project Context)
    /// </summary>
    public class AgentRegistry: RegistryBase<InteractionAgent>, ITickable
    {
        private readonly InteractableRegistry _interactableRegistry;
        private readonly AvailableInteractions.Factory _availableInteractionsFactory;

        private Dictionary<InteractionAgent, AvailableInteractions> _inRangeInteractables =
            new Dictionary<InteractionAgent, AvailableInteractions>();

        public AgentRegistry(AvailableInteractions.Factory availableInteractionsFactory) {
            _availableInteractionsFactory = availableInteractionsFactory;
        }
        protected override void ValueAdded(InteractionAgent value)
        {
            if (!_inRangeInteractables.ContainsKey(value))
            {
                _inRangeInteractables.Add(value, _availableInteractionsFactory.Create(value));
            }
        }
        protected override void ValueRemoved(InteractionAgent value)
        {
            if (_inRangeInteractables.ContainsKey(value))
            {
                _inRangeInteractables.Remove(value);
            }
        }
        public void Tick()
        {
            foreach (var agent in Values)
            {
                if (agent == null) continue;
                _inRangeInteractables[agent].UpdateInteractions(agent);
            }
        }
    }

    /// <summary>
    /// keeps track of interactables and their interaction ranges
    /// GLOBAL SINGLETON (bound in Project Context)
    /// </summary>
    public class InteractableRegistry : RegistryBase<Interactable>
    {
        private Dictionary<Interactable, float> _distanceLookup = new Dictionary<Interactable, float>();
        protected override void ValueAdded(Interactable value)
        {
            float distSqr = value.range * value.range;
            _distanceLookup.Add(value, distSqr);
            base.ValueAdded(value);
        }

        protected override void ValueRemoved(Interactable value)
        {
            _distanceLookup.Remove(value);
        }

        public IEnumerable<(Interactable, bool)> GetInteractablesInRange(InteractionAgent agent)
        {
            foreach (var interactable in Values)
            {
                if (interactable == null) continue;
                var dist = (agent.transform.position - interactable.transform.position).sqrMagnitude;
                bool inRange = dist <= _distanceLookup[interactable];
                yield return (interactable, inRange);
            }
        }
    }
}