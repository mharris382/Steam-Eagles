﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using Players;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Buildings
{
    /// <summary>
    /// keeps track of which structures the players are currently inside.
    ///
    /// consider using this alongside a room manager for room tracking within
    /// a particular building 
    /// <para>
    /// Note: player asset objects are loaded from addressables in start
    /// </para>
    /// 
    /// </summary>
    public class StructureManager : Singleton<StructureManager>
    {

        public Player[] players;
        public override bool DestroyOnLoad => false;

        //the structures the players are currently inside (index is player id)
        private List<StructureState>[] _playerStructures;
        
        private bool _isInitialized;
        public bool IsInitialized => _isInitialized;

        protected override void OnCreatedFromScript()
        {
            
        }

        protected override void Init()
        {
            _isInitialized = false;
            _playerStructures = new List<StructureState>[2]
            {
                new List<StructureState>(),
                new List<StructureState>()
            };
        }


        
        private IEnumerator Start()
        {
            yield return null;
            if (players == null)
            {
                var loadOp = Addressables.LoadAssetsAsync<Player>("players", obj => Debug.Log("Structure Manager found player: " + obj.name));
                yield return loadOp;
                players = loadOp.Result.ToArray();
            }
            _isInitialized = true;
            
            for (int i = 0; i < _playerStructures.Length; i++)
            {
                if (_playerStructures[i] != null)
                {
                    foreach (var structureState in _playerStructures[i])
                    {
                        if (!_playerStructures[i].Contains(structureState))
                            NotifyPlayerEnteredStructure(players[i], structureState);
                    }
                }
            }
        }


        public void NotifyPlayerEnteredStructure(int playerNumber, StructureState structureState)
        {
            if(_playerStructures[playerNumber].Contains(structureState)) return;
            _playerStructures[playerNumber].Add(structureState);
            if (_isInitialized)
            {
                Debug.Assert(structureState != null);
                if (!_playerStructures[playerNumber].Contains(structureState))
                    NotifyPlayerEnteredStructure(players[playerNumber], structureState);
            }
        }
        public void NotifyPlayerExitedStructure(int playerNumber, StructureState structureState)
        {
            if(!_playerStructures[playerNumber].Contains(structureState)) return;
            _playerStructures[playerNumber].Remove(structureState);
            if (_isInitialized)
            {
                Debug.Assert(structureState != null);
                if (_playerStructures[playerNumber].Contains(structureState))
                    NotifyPlayerExitedStructure(players[playerNumber], structureState);
            }
        }
        
        
        private void NotifyPlayerEnteredStructure(Player player, StructureState structureState)
        {
            if (_playerStructures[player.playerNumber].Contains(structureState))
            {
                Debug.LogError($"Why is player {player.characterTag} entering a structure they can already edit?", structureState);
            }
            else
            {
                _playerStructures[player.playerNumber].Add(structureState);
            }
            UpdatePlayerEditableStructure(player);
        }
        private void NotifyPlayerExitedStructure(Player player, StructureState structureState)
        {
            UpdatePlayerEditableStructure(player);
        }

        private void UpdatePlayerEditableStructure(Player player)
        {
            if (player == null)
            {
                Debug.LogError("Player is null!", this);
                return;
            }
           // return;
            if (_playerStructures[player.playerNumber].Count == 0)
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
            var structures = _playerStructures[p.playerNumber];
            if(structures.Count == 0) return null;
            return structures.OrderBy(t => t.structureEditPriority).First();
            throw new System.NotImplementedException();
        }
        
    }
}