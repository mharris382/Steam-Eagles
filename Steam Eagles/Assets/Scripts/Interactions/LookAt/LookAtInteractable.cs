using Cysharp.Threading.Tasks;
using Interactions.Installers;
using UnityEngine;
using Zenject;

namespace Interactions.LookAt
{
    public class LookAtInteractable : Interactable
    {
        public GameObject vCam;
        private PlayerVCamFactory _playerVCamFactory;

        public async override UniTask<bool> Interact(InteractionAgent agent)
        {
            var vCam = _playerVCamFactory.Create(agent, this.vCam);
            if(vCam == null)
            {
                return false;
            }
            vCam.SetActive(true);
            await UniTask.WaitUntil(() => agent.CancelPressed);
            Destroy(vCam);
            return true;
        }
        
        [Inject]
        public void InjectMe(PlayerVCamFactory playerVCamFactory)
        {
            _playerVCamFactory = playerVCamFactory;
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