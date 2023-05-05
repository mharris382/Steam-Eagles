using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Players.PCController.ParallaxSystems
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ParallaxSprite : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        public bool includeChildren = true;
        private ParallaxSprites _parallaxSprites;

        public SpriteRenderer SpriteRenderer => _spriteRenderer ??= GetComponent<SpriteRenderer>();

        private SpriteRenderer[] _spriteRenderers;
        
        [Inject] public void InjectMe(ParallaxSprites parallaxSprites)
        {
            _parallaxSprites = parallaxSprites;
            parallaxSprites.AddSprite(this);
        }
        
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
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

        
        
        public IEnumerable<SpriteRenderer> GetSpriteRenderers()
        {
            if (includeChildren)
            {
                if (_spriteRenderers == null)
                {
                    _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
                }

                foreach (var sr in _spriteRenderers)
                {
                    yield return sr;
                }
            }
            else
                yield return SpriteRenderer;
        }
    }
}