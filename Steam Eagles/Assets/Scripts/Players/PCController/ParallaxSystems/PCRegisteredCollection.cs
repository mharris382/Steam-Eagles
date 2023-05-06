#nullable enable
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Players.PCController.ParallaxSystems
{
    public abstract class PCRegisteredCollection<T> : RegisteredCollection<T> where T : Component
    {
        private Dictionary<T, T>[] _playerCopies;


        public PCRegisteredCollection()
        {
            _playerCopies = new Dictionary<T, T>[2]
            {
                new Dictionary<T, T>(),
                new Dictionary<T, T>()
            };
        }
        public sealed override void OnObjectAdded(T sprite)
        {
            if(_items.Contains(sprite)) return;
            _items.Add(sprite);
            int cnt = 0;
            foreach (var playerSpriteCopy in _playerCopies)
            {
                if(!playerSpriteCopy.TryGetValue(sprite, out var copy))
                {
                    copy = Object.Instantiate(sprite, sprite.transform.parent);
                    playerSpriteCopy.Add(sprite, copy);
                }

                SetCopyVisible(copy, cnt, false);
                cnt++;
            }
            HideOriginal(sprite);
        }

        protected virtual void SetCopyVisible(T copy, int player, bool visible)
        {
            copy.gameObject.SetActive(visible);
        }

        protected virtual void HideOriginal(T original)
        {
            original.gameObject.SetActive(false);
        }

        public sealed override void OnObjectRemoved(T sprite)
        {
            foreach (var playerSpriteCopy in _playerCopies)
            {
                if(!playerSpriteCopy.ContainsKey(sprite)) continue;
                if(playerSpriteCopy[sprite].gameObject!=null) playerSpriteCopy[sprite].gameObject.SetActive(false);
                else playerSpriteCopy.Remove(sprite);
            }
        }
        public T GetCopyForPlayer(T original, int playerNumber)
        {
            var lookup = _playerCopies[playerNumber];
            if (!lookup.TryGetValue(original, out var copy))
            {
                copy = CreateCopy(original, playerNumber);
                lookup.Add(original, copy);
            }
            return copy;
        }
        protected abstract T CreateCopy(T original, int playerNumber);
        public sealed override int CountObjects() => _items.Sum(CountObject);
        protected virtual int CountObject(T obj) => 1;
        
        

        public virtual IEnumerable<Transform> GetOriginals() => _items.Select(t => t.transform);
        public virtual IEnumerable<T> GetCopies(int player) => _items.Select(t => GetCopyForPlayer(t, player));
    }
}