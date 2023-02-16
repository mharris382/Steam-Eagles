using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoreLib.Pickups
{
    [CreateAssetMenu(fileName = "New Pickup", menuName = "Steam Eagles/New Pickup", order = 0)]
    public class Pickup : ScriptableObject
    {
        [ValidateInput(nameof(ValidateKey))]
        public string key;
        
        [Required, AssetsOnly]
        public PickupBody prefab;


        [ShowInInspector, ReadOnly, PreviewField(ObjectFieldAlignment.Right, Height = 200)]
        public Sprite DefaultSprite
        {
            get
            {
                if (prefab == null) return null;
                return prefab.SpriteRenderer.sprite;
            }
        }
        [PreviewField(ObjectFieldAlignment.Right, Height = 150)]
        public Sprite[] overrideSprites;
        public PickupBody SpawnPickup(Vector3 position)
        {
            var inst = Instantiate(prefab, position, Quaternion.identity);

            return inst;
        }

        private bool ValidateKey(string key)
        {
            return !String.IsNullOrEmpty(key);
        }
    }
}