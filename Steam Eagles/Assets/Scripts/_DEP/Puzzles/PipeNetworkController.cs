using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

namespace Puzzles
{
    public class PipeNetworkController : MonoBehaviour
    {
        [SerializeField] private SharedTilemap pipeTilemap;
        [SerializeField] private SharedTilemap solidTilemap;
        [SerializeField] private Tilemap reactorTilemap;
        
        private void Awake()
        {
            MessageBroker.Default.Receive<BuildActionInfo>().TakeUntilDestroy(this).Subscribe(OnPipeAdded);
            MessageBroker.Default.Receive<DisconnectActionInfo>().TakeUntilDestroy(this).Subscribe(OnPipeRemoved);
        }


        void OnPipeAdded(BuildActionInfo buildActionInfo)
        {
            Debug.Log(buildActionInfo.ToString().Bolded().ColoredBlue());
        }

        void OnPipeRemoved(DisconnectActionInfo disconnectActionInfo)
        {
            Debug.Log(disconnectActionInfo.ToString().InItalics().ColoredRed());
        }
    }
}