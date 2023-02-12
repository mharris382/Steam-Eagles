using System;
using CoreLib;
using PhysicsFun.Buildings;
using UniRx;
using UnityEngine;

namespace Buildings
{
    public class StructureState : MonoBehaviour
    {
        [Tooltip("When a player is inside multiple structures, the one with the highest priority will be used for placing and removing blocks")]
        public int structureEditPriority;
        public bool[] playersInBuilding;

        private Subject<int> _playerCountChanged = new Subject<int>();
        
        public IObservable<int> PlayerCountChanged => _playerCountChanged;
        private IStructure _structure;
        
        
        public bool HasPlayersInBuilding()
        {
            return playersInBuilding[0] || playersInBuilding[1];
        }
        
        public bool HasPlayerInBuilding(int playerIndex)
        {
            return playersInBuilding[playerIndex];
        }
        
        private void Awake()
        {
            playersInBuilding = new bool[2]
            {
                false,
                false
            };
            _structure = GetComponent<IStructure>();
            Debug.Assert(_structure!=null, $"Missing Structure on {name}" , this);
        }

        public void SetPlayerInBuilding(int playerNumber, bool playerInBuilding)
        {
            playerNumber = Mathf.Clamp(playerNumber, 0, 1);
            
            if (playerInBuilding)
            {
                StructureManager.Instance.NotifyPlayerEnteredStructure(playerNumber, this);
            }
            else
            {
                StructureManager.Instance.NotifyPlayerExitedStructure(playerNumber, this);
            }
            playersInBuilding[playerNumber] = playerInBuilding;
            _playerCountChanged.OnNext(GetPlayerCount());
        }

        public int GetPlayerCount()
        {
            int cnt = 0;
            foreach (var b in playersInBuilding)
            {
                if (b) cnt++;
            }
            return cnt;
        }

      


        public EditableTilemapStructure GetEditableStructure()
        {
            return new EditableTilemapStructure()
            {
                solidTilemap = _structure.SolidTilemap.Tilemap,
                foundationTilemap = _structure.FoundationTilemap.Tilemap,
                pipeTilemap = _structure.PipeTilemap.Tilemap,
                wallTilemap = _structure.WallTilemap.Tilemap
            };
        }
    }
}