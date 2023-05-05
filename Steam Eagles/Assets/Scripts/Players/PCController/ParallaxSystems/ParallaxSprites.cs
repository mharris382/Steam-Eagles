﻿using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Players.PCController.ParallaxSystems
{
    public class ParallaxSprites
    {

        private int _spriteCount;
        private readonly List<ParallaxSprite> _sprites = new List<ParallaxSprite>();
        /// <summary>
        /// this dictionary stores a list of copies of each sprite for each player. each copy is only rendered for the player it belongs to,
        /// while the original is not rendered for any player
        /// </summary>
        private readonly Dictionary<ParallaxSprite, ParallaxSprite>[] _playerSpriteCopies;
        private readonly Subject<int> _onSpriteAdded = new Subject<int>();
        private readonly Subject<int> _onSpriteRemoved = new Subject<int>();
        private readonly int[] _layers;
        
        public IObservable<int> OnSpriteAdded => _onSpriteAdded;
        public IObservable<int> OnSpriteRemoved => _onSpriteRemoved;
        public IObservable<int> OnSpriteChanged => _onSpriteAdded.Merge(_onSpriteRemoved);

        public int Count => _spriteCount;

        public ParallaxSprites()
        {
            _sprites = new List<ParallaxSprite>();
            _playerSpriteCopies = new Dictionary<ParallaxSprite, ParallaxSprite>[2]
            {
                new Dictionary<ParallaxSprite, ParallaxSprite>(),
                new Dictionary<ParallaxSprite, ParallaxSprite>()
            };
            _layers = new int[2]
            {
                LayerMask.NameToLayer("P1"),
                LayerMask.NameToLayer("P2"),
            };
        }

        public void AddSprite(ParallaxSprite sprite)
        {
            if(_sprites.Contains(sprite)) return;
            _sprites.Add(sprite);
            int cnt = 0;
            foreach (var playerSpriteCopy in _playerSpriteCopies)
            {
                if(!playerSpriteCopy.TryGetValue(sprite, out var copy))
                {
                    copy = Object.Instantiate(sprite, sprite.transform.parent);
                    playerSpriteCopy.Add(sprite, copy);
                }

                SetSpriteVisibleToPlayer(copy, cnt);
                cnt++;
            }
            //set sprite visible to no player
            SetSpriteVisibleToPlayer(sprite, -1);
            Recount();
            _onSpriteAdded.OnNext(_spriteCount);
        }

        void Recount() => _spriteCount = _sprites.Sum(t => t.GetSpriteRenderers().Count());


        private void SetSpriteVisibleToPlayer(ParallaxSprite sprite, int player)
        {
            switch (player)
            {
                case 0:
                case 1:
                    foreach (var spriteRenderer in sprite.GetSpriteRenderers())
                    {
                        spriteRenderer.gameObject.layer = _layers[player];
                    }
                    break;
                default:
                    foreach (var spriteRenderer in sprite.GetSpriteRenderers())
                    {
                        spriteRenderer.gameObject.layer = LayerMask.NameToLayer("TransparentFX");
                    }
                    break;
            }
        }

        public void RemoveSprite(ParallaxSprite sprite)
        {
            
            foreach (var playerSpriteCopy in _playerSpriteCopies)
            {
                if(!playerSpriteCopy.ContainsKey(sprite)) continue;
                if(playerSpriteCopy[sprite].gameObject!=null)
                    playerSpriteCopy[sprite].gameObject.SetActive(false);
                else
                    playerSpriteCopy.Remove(sprite);
            }
            if (!_sprites.Contains(sprite)) return;
            _sprites.Remove(sprite);
            Recount();
            _onSpriteRemoved.OnNext(_spriteCount);
        }
        public IEnumerable<ParallaxSprite> GetSpritesForPlayer(int playerNumber)
        {
            var lookup = _playerSpriteCopies[playerNumber];
            foreach (var parallaxSprite in _sprites)
            {
                yield return lookup[parallaxSprite];
            }
        }
        public IEnumerable<SpriteRenderer> GetSpriteRendererForPlayer(int playerNumber) => GetSpritesForPlayer(playerNumber).SelectMany(t => t.GetSpriteRenderers());


        public IEnumerable<Transform> GetOriginals() => _sprites.SelectMany(t => t.GetSpriteRenderers()).Select(t => t.transform);
        public IEnumerable<Transform> GetCopies(int player) => GetSpritesForPlayer(player).SelectMany(t => t.GetSpriteRenderers()).Select(t => t.transform);
        public Dictionary<ParallaxSprite, ParallaxSprite> GetPlayerCopies(int playerNumber) => _playerSpriteCopies[playerNumber];
    }
}