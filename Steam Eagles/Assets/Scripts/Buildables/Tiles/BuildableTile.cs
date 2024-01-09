using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Items;
using SteamEagles.Characters;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using Zenject;

namespace Buildables.Tiles
{
    [RequireComponent(typeof(RecipeInstance))]
    [RequireComponent(typeof(ZenAutoInjecter))]
    public class BuildableTile : BuildableMachineBase
    {
        public override Vector2Int MachineGridSize => Vector2Int.one;

        List<CharacterState> _charactersOnTile = new List<CharacterState>();

        private void Awake()
        {
            var box = GetComponentInChildren<BoxCollider2D>();
            Debug.Assert(box != null, "Missing box collider on buildable tile", this);
            
            box.OnCollisionEnter2DAsObservable()
                .Select(t => t.collider.attachedRigidbody.GetComponent<CharacterState>())
                .Where(t => t != null)
                .Subscribe(_OnCharacterEntered).AddTo(this);
            
            box.OnCollisionExit2DAsObservable()
                .Select(t => t.collider.attachedRigidbody.GetComponent<CharacterState>())
                .Where(t => t != null)
                .Subscribe(_OnCharacterExit).AddTo(this);
        }

        private void OnDestroy()
        {
            foreach(var character in _charactersOnTile)
                OnCharacterExit(character);
        }

        private void Update()
        {
            foreach (var character in _charactersOnTile)
            {
                if(character == null)
                    continue;
                OnCharacterStay(character);
            }
        }

        void _OnCharacterEntered(CharacterState character)
        {
            if(_charactersOnTile.Contains(character))
                return;
            _charactersOnTile.Add(character);
            OnCharacterEntered(character);
        }
      
        
      

        void _OnCharacterExit(CharacterState character)
        {
            _charactersOnTile.Remove(character);
            OnCharacterExit(character);
        }
        
        protected virtual void OnCharacterEntered(CharacterState character)
        {
            Debug.Log($"Character entered {character.name} on {name}"); // TODO: Remove this debug log
        }
        
        protected virtual void OnCharacterStay(CharacterState character)
        {
            Debug.Log($"Character standing on {character.name} on {name}"); // TODO: Remove this debug log
        }
        protected virtual void OnCharacterExit(CharacterState character)
        {
            Debug.Log($"Character exited {character.name} on {name}"); // TODO: Remove this debug log
        }
    }
}