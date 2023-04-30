using System;
using Players.Shared;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace UI
{
    public class UISelection : MonoBehaviour
    {
        public GraphicRaycaster raycaster;
        public EventSystem eventSystem;
        public VirtualMouseInput mouseInput;
        private RectTransform _rectTransform;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (raycaster == null) raycaster = GetComponentInParent<GraphicRaycaster>();
            if (raycaster == null) Debug.LogError("Missing raycaster for selection", this);


        }

        public void Update()
        {
            if (!HasResources()) 
                return;
            
        }

        public bool HasResources()
        {
            if (raycaster == null)
                return false;

            if (eventSystem == null)
            {
                eventSystem = GameObject.FindObjectOfType<EventSystem>();
                if (eventSystem == null) return false;
            }

            return true;
        }
    }
}