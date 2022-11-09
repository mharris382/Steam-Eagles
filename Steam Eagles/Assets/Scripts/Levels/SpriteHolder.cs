using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Levels
{
    public class SpriteHolder : MonoBehaviour
    {
        private SpriteRenderer _heldSprite;
    
        [SerializeField] private string heldLayerName = "Held";
        [SerializeField] private int heldOrderInLayer = 1;


        private bool _overrideSortingLayer;
        private int _heldLayerID;
        private int _previousOrderInLayer;
        private int _previousSortingLayerID;
    
        public GameObject HeldObject
        {
            get => _heldSprite.gameObject;
            set
            {
                if (_heldSprite != null)
                {
                    Debug.LogWarning("Can only hold one object at a time", this);
                    return;
                }
                if (value == null)
                {
                    ReleaseHeldSprite();
                }
                else
                {
                    _heldSprite = value.GetComponent<SpriteRenderer>();
                    if (_heldSprite != null)
                    {
                        HoldSprite(_heldSprite);
                    }
                }
            }
        }

        private void Awake()
        {
            _heldLayerID = SortingLayer.NameToID(heldLayerName);
            Debug.Assert(_heldLayerID != 0, $"Sorting layer named {heldLayerName} not found");
        }

        private void ReleaseHeldSprite()
        {
            throw new System.NotImplementedException();
        }

        private void HoldSprite(SpriteRenderer spriteRenderer)
        {
            _previousSortingLayerID = spriteRenderer.sortingLayerID;
            _previousOrderInLayer = spriteRenderer.sortingOrder;
        
        }
    }
}