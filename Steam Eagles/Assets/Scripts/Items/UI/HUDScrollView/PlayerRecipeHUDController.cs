using System;
using System.Collections;
using Characters;
using CoreLib;
using CoreLib.Entities;
using Game;
using Sirenix.OdinInspector;
using UI.PlayerGUIs;
using UniRx;
using UnityEngine;

namespace Items.UI.HUDScrollView
{
    public class PlayerRecipeHUDController : ToolRecipeHUDController
    {
        private PlayerCharacterGUIController _guiController;


        [Required]
        public Tool testTool;


        private Inventory _backpack;
        private IEnumerator _waitForEntitySetup;

        public bool invertSelection;
        
        
        private PlayerCharacterGUIController guiController => _guiController == null
            ? _guiController = GetComponentInParent<PlayerCharacterGUIController>()
            : _guiController;

        void Awake()
        {
            guiController.PcEntityProperty.Subscribe(OnPcEntityChange).AddTo(this);
            guiController.ShowRecipeHUD.Subscribe(gameObject.SetActive).AddTo(this);
            gameObject.SetActive(guiController.ShowRecipeHUD.Value);
            
        }

        bool HasResources() => guiController != null && guiController.HasAllResources();

        public Inventory GetBackpack()
        {
            if (!HasResources())
                return null;
            if (guiController.pcEntity.LinkedGameObject == null)
            {
                Debug.LogError("Supposed to have all resources, but no linked game object", guiController.pcEntity);
                _backpack = null;
                return null;
            }

            if (_backpack != null)
            {
                return _backpack;
            }

            var allInventories = guiController.pcEntity.LinkedGameObject.GetComponentsInChildren<Inventory>();
            ToolState toolState = guiController.pcEntity.LinkedGameObject.GetComponent<ToolState>();
            Inventory mainInventory = null, toolbeltInventory = null;
            foreach (var i in allInventories)
            {
                if (i.isMain) mainInventory = i;
                else if (i.isToolbelt) toolbeltInventory = i;
            }

            return (_backpack = mainInventory);
        }


        void OnPcEntityChange(Entity e)
        {
            if (_waitForEntitySetup != null)
                StopCoroutine(_waitForEntitySetup);
            _backpack = null;
            _waitForEntitySetup = WaitForEntitySetup(e);
        }

        IEnumerator WaitForEntitySetup(Entity e)
        {
            while (e.LinkedGameObject == null)
            {
                yield return null;
            }

            var character = e.LinkedGameObject.GetComponent<Character>();
            while (!character.IsEntityInitialized)
            {
                Debug.Log($"Waiting for character {character.name} to be initialized",this);
                yield return null;
            }

            var backpack = GetBackpack();
            Debug.Assert(backpack != null, "Backpack is null");
            Setup(testTool, backpack);
            _waitForEntitySetup = null;
        }

        private bool inited = false;
        
        private float _timeLastUpdated;
        const float UpdateInterval = 0.25f;
        private void Update()
        {
            if (guiController.HasAllResources() == false)
            {
                inited = false;
                return;
            }

            if (inited == false)
            {
                inited = true;
                var backpack = GetBackpack();
                Debug.Assert(backpack != null, "Backpack is null");
                Setup(testTool, backpack);
            }
            var inputPlayer = guiController.playerInput;
            
            var recipeSelect = inputPlayer.actions["Select Recipe"].ReadValue<float>();
            if (recipeSelect != 0)
            {
                if(Time.realtimeSinceStartup - _timeLastUpdated > UpdateInterval)
                    _timeLastUpdated = Time.realtimeSinceStartup;
                else
                    return;
                if (recipeSelect > 0)
                {
                    if(invertSelection)scrollView.SelectPrev();
                    else scrollView.SelectNext();
                }
                else
                {
                    if(invertSelection)scrollView.SelectNext();
                    else scrollView.SelectPrev();
                }
            }
        }
    }
}