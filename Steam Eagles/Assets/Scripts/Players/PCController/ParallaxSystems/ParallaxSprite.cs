using System;
using System.Collections.Generic;
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

        
        [Inject] public void InjectMe(ParallaxSprites parallaxSprites)
        {
            _parallaxSprites = parallaxSprites;
            parallaxSprites.AddSprite(this);
        }
        
        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
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
                foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
                    yield return spriteRenderer;
            else
                yield return SpriteRenderer;
        }
    }
}