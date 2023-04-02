using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace Characters.Interactables
{
    [System.Obsolete("Interaction System will be replaced with a generic version to be used by both NPC (AI) and PC (players)")]
    /// <summary>
    /// handles making interactions available to interaction controller
    /// </summary>
    public class InteractionManager : MonoBehaviour
    {
        public List<InteractionController> controllers;
        public List<InteractableObject> interactableObjects;


        private static InteractionManager _instance;

        public static InteractionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<InteractionManager>();
                    if (_instance == null)
                    {
                        _instance = new GameObject("InteractionManager").AddComponent<InteractionManager>();
                    }
                }

                return _instance;
            }
        }

        public bool forceOn;

        private void Awake()
        {
            if (_instance == null || forceOn)
            {
                _instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        private void Update()
        {
            if (controllers == null) return;
            foreach (var controller in controllers)
            {
                float smallestDistSqr = float.MaxValue;
                InteractableObject bestObj = null;

                foreach (var obj in interactableObjects)
                {
                    Vector2 diff = (obj.Position - controller.Position);
                    if (diff.sqrMagnitude < smallestDistSqr && diff.sqrMagnitude < controller.interactionRadius)
                    {
                        bestObj = obj;
                        smallestDistSqr = diff.sqrMagnitude;
                    }
                }

                controller.CurrentAvailableInteractable = bestObj;
            }
        }
    }
}