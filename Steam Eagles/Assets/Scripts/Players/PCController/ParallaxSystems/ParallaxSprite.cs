using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Players.PCController.ParallaxSystems
{
    [RequireComponent(typeof(Renderer))]
    public class ParallaxSprite : MonoBehaviour
    {
        private Renderer _spriteRenderer;
        public bool includeChildren = true;
        private ParallaxSprites _parallaxSprites;

        public Renderer SpriteRenderer => _spriteRenderer ??= GetComponent<Renderer>();

        private Renderer[] _spriteRenderers;
        
        [Inject] public void InjectMe(ParallaxSprites parallaxSprites)
        {
            _parallaxSprites = parallaxSprites;
            parallaxSprites.AddSprite(this);
        }
        
        private void Awake()
        {
            _spriteRenderer = GetComponent<Renderer>();
            _spriteRenderers = GetComponentsInChildren<Renderer>();
            var l = _spriteRenderers.ToList();
            l.Remove(_spriteRenderer);
            _spriteRenderers = l.ToArray();
        }

        private void OnEnable()
        {
            if (_parallaxSprites != null)
                _parallaxSprites.AddSprite(this);
            else
                Debug.Log("ParallaxSprites is null");
        }

        private void OnDisable()
        {
            if (_parallaxSprites != null)
                _parallaxSprites.RemoveSprite(this);
            else
                Debug.Log("ParallaxSprites is null");
        }

        public IEnumerable<Renderer> GetRenderers()
        {
            if (includeChildren)
            {
                if (_spriteRenderers == null)
                {
                    _spriteRenderers = GetComponentsInChildren<Renderer>();
                }

                foreach (var sr in _spriteRenderers)
                {
                    yield return sr;
                }
            }
            else
                yield return SpriteRenderer;
        }
        
        public IEnumerable<SpriteRenderer> GetSpriteRenderers()
        {
            foreach (var renderer in GetRenderers())
            {
                if(renderer is SpriteRenderer sr)
                    yield return sr;
            }
        }
    }
}