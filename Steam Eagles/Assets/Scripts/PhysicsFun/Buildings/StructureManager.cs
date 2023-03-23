using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using CoreLib;
using Players;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace PhysicsFun.Buildings
{
    public class StructureManager : Singleton<StructureManager>
    {

        public Player[] players;

        private Dictionary<Player, List<StructureState>> _playerEditableStructures;


        protected override void OnCreatedFromScript()
        {
            
        }

        protected override void Init()
        {
           
        }


        private IEnumerator Start()
        {
            yield return null;
            if (players == null)
            {
                var loadOp = Addressables.LoadAssetsAsync<Player>("player", obj => Debug.Log("Structure Manager found player: " + obj.name));
                yield return loadOp;
                players = loadOp.Result.ToArray();
            }
            _playerEditableStructures = new Dictionary<Player, List<StructureState>>();
            foreach (var player in players)
            {
                _playerEditableStructures.Add(player, new List<StructureState>());
                UpdatePlayerEditableStructure(player);
            }
        }


        public void NotifyPlayerEnteredStructure(int playerNumber, StructureState structureState)
        {
            Debug.Assert(structureState != null);
            NotifyPlayerEnteredStructure(players[playerNumber], structureState);
        }
        public void NotifyPlayerExitedStructure(int playerNumber, StructureState structureState)
        {
            Debug.Assert(structureState != null);
            NotifyPlayerExitedStructure(players[playerNumber], structureState);
        }
        
        
        private void NotifyPlayerEnteredStructure(Player player, StructureState structureState)
        {
            if (_playerEditableStructures[player].Contains(structureState))
            {
                Debug.LogError($"Why is player {player.characterTag} entering a structure they can already edit?", structureState);
            }
            else
            {
                _playerEditableStructures[player].Add(structureState);
            }
            UpdatePlayerEditableStructure(player);
        }
        private void NotifyPlayerExitedStructure(Player player, StructureState structureState)
        {
            if (!_playerEditableStructures[player].Contains(structureState))
            {
                Debug.LogError($"Why is player {player.characterTag} exiting a structure are not inside?", structureState);
                return;
            }
            _playerEditableStructures[player].Remove(structureState);
            UpdatePlayerEditableStructure(player);
        }

        private void UpdatePlayerEditableStructure(Player player)
        {
            if (_playerEditableStructures[player].Count == 0)
            {
                Debug.Log($"Disabled Structure editing for player {player.name}({player.characterTag})");
                player.DisableStructureEditing();
            }
            else
            {
                var nextStructure = GetHighestPriorityStructure(player);
                Debug.Assert(nextStructure != null, "Editable structure list is not empty but no structure was found!");
                Debug.Log($"Enabled Structure editing for player {player.name}({player.characterTag}) on {nextStructure.name}");
                player.EnableStructureEditing(nextStructure.GetEditableStructure());
            }
        }
        
        private StructureState GetHighestPriorityStructure(Player p)
        {
            var structures = _playerEditableStructures[p];
            if(structures.Count == 0) return null;
            return structures.OrderBy(t => t.structureEditPriority).First();
            throw new System.NotImplementedException();
        }
        
    }
}