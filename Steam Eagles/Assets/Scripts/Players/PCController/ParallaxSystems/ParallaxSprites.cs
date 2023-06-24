using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Players.PCController.ParallaxSystems
{
    public class ParallaxSprites : IDisposable
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
            if (_sprites.Contains(sprite)) return;
            _sprites.Add(sprite);
            int cnt = 0;
            foreach (var playerSpriteCopy in _playerSpriteCopies)
            {
                if (!playerSpriteCopy.TryGetValue(sprite, out var copy))
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

        private void Recount() => _spriteCount = _sprites.Sum(t => t.GetRenderers().Count());


        private void SetSpriteVisibleToPlayer(ParallaxSprite sprite, int player)
        {
            if (sprite == null) return;
            switch (player)
            {
                case 0:
                case 1:
                    foreach (var spriteRenderer in sprite.GetRenderers().Where(t => t != null))
                    {
                        spriteRenderer.gameObject.layer = _layers[player];
                    }

                    break;
                default:
                    foreach (var spriteRenderer in sprite.GetRenderers().Where(t => t != null))
                    {
                        spriteRenderer.gameObject.layer = LayerMask.NameToLayer("TransparentFX");
                    }

                    break;
            }
        }

        public void RemoveSprite(ParallaxSprite sprite)
        {
            if (sprite == null) return;
            foreach (var playerSpriteCopy in _playerSpriteCopies)
            {
                if (!playerSpriteCopy.ContainsKey(sprite)) continue;
                try
                {
                    if (playerSpriteCopy[sprite].gameObject != null)
                        playerSpriteCopy[sprite].gameObject.SetActive(false);
                    else
                        playerSpriteCopy.Remove(sprite);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    playerSpriteCopy.Remove(sprite);
                }
            }

            if (!_sprites.Contains(sprite)) return;
            _sprites.Remove(sprite);
            Recount();
            _onSpriteRemoved.OnNext(_spriteCount);
        }

        public IEnumerable<ParallaxSprite> GetSpritesForPlayer(int playerNumber)
        {
            var lookup = _playerSpriteCopies[playerNumber];
            int nullCount = 0;
            foreach (var parallaxSprite in _sprites)
            {
                if (parallaxSprite == null)
                {
                    nullCount++;
                    continue;
                }
                yield return lookup[parallaxSprite];
            }

            if (nullCount > 0) _sprites.RemoveAll(t => t == null);
        }

        public IEnumerable<Renderer> GetSpriteRendererForPlayer(int playerNumber) =>
            GetSpritesForPlayer(playerNumber).SelectMany(t => t.GetRenderers());


        public IEnumerable<Transform> GetOriginals() =>
            _sprites.Where(t => t != null).SelectMany(t => t.GetRenderers()).Select(t => t.transform);

        public IEnumerable<Transform> GetCopies(int player) => GetSpritesForPlayer(player).Where(t => t != null)
            .SelectMany(t => t.GetRenderers()).Select(t => t.transform);

        public Dictionary<ParallaxSprite, ParallaxSprite> GetPlayerCopies(int playerNumber) =>
            _playerSpriteCopies[playerNumber];

        public void Dispose()
        {
            _onSpriteAdded?.Dispose();
            _onSpriteRemoved?.Dispose();
            _sprites.RemoveAll(t => t == null);
            foreach (var playerSpriteCopy in _playerSpriteCopies)
            {
                foreach (var parallaxSprite in playerSpriteCopy)
                    if (parallaxSprite.Value != null)
                        Object.Destroy(parallaxSprite.Value.gameObject);
                playerSpriteCopy.Clear();
            }
        }
    }
}