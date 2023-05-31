using System;
using CoreLib.Interfaces;
using Cysharp.Threading.Tasks;
using Interactions.Installers;
using UnityEngine;
using Zenject;

namespace Interactions.LookAt
{
    public class LookAtInteractable : Interactable
    {
        private PlayerViewFactory _viewFactory;
        public float minLookTime = 1f;

        public override async UniTask<bool> Interact(InteractionAgent agent)
        {
            var vCam = _viewFactory.GetViewForPlayer(agent.gameObject, this.virtualCamera);
            if(vCam == null)
            {
                return false;
            }
            await UniTask.Delay(TimeSpan.FromSeconds(minLookTime));
            vCam.SetActive(true);
            await UniTask.WaitUntil(() => agent.CancelPressed || agent.InteractPressed);
            Destroy(vCam);
            return true;
        }
        
        [Inject]
        public void InjectMe(PlayerViewFactory viewFactory)
        {
            _viewFactory = viewFactory;
        }
    }

    public class LayerMaskFactory : IFactory<int, LayerMask>
    {
        private LayerMask[] _pcLayers;
        public LayerMaskFactory()
        {
            _pcLayers = new LayerMask[2]
            {
                LayerMask.NameToLayer("P1"),
                LayerMask.NameToLayer("P2")
            };
        }
        public LayerMask Create(int param) => _pcLayers[param];
    }


}