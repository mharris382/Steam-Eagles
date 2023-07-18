using System;
using System.Collections;
using System.Linq;
using Characters;
using CoreLib;
using CoreLib.MyEntities;
using Cysharp.Threading.Tasks;
using Game;
using SteamEagles.Characters;
using UI.PlayerGUIs;
using UniRx;
using UnityEngine;

namespace Items.UI.HUDScrollView
{
    public class PlayerRecipeHUDController : ToolRecipeHUDController
    {
        private ToolState _toolState;
        private PlayerCharacterGUIController _guiController;
        private Inventory _backpack;
        private Coroutine _waitForEntitySetup;

        private bool _initialized = false;
        private float _timeLastUpdated;



        private PlayerCharacterGUIController GUIController => _guiController == null
            ? _guiController = GetComponentInParent<PlayerCharacterGUIController>()
            : _guiController;

        private ToolState ToolState
        {
            get
            {
                if (_toolState == null)
                {
                    if (GUIController != null && GUIController.PlayerCharacter != null)
                    {
                        _toolState = GUIController.PlayerCharacter.GetComponent<ToolState>();
                    }
                }
                //added this bit in case the player character changes at some point
                else if (_toolState.gameObject != GUIController.PlayerCharacter)
                {
                    _toolState = GUIController.PlayerCharacter ? GUIController.PlayerCharacter.GetComponent<ToolState>() : null;
                }
                return _toolState;
            }
        }

        private void Awake()
        {
            // GUIController.PcEntityProperty.Subscribe(OnPcEntityChange).AddTo(this);
            // GUIController.ShowRecipeHUD.Subscribe(gameObject.SetActive).AddTo(this);
            gameObject.SetActive(GUIController.ShowRecipeHUD.Value);
            
        }

        private bool HasResources() => GUIController != null && GUIController.HasAllResources()  && ToolState != null;


        private void OnSetup()
        {
            if (_waitForEntitySetup != null)
                StopCoroutine(_waitForEntitySetup);
            _backpack = null;
            _waitForEntitySetup = StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                await UniTask.WaitUntil(() => GUIController.PlayerCharacter != null);
                var initializer = GUIController.PlayerCharacter.GetComponent<EntityInitializer>();
                await UniTask.WaitUntil(() => initializer.isDoneInitializing);
                _backpack = GetBackpack();
                
            }));
        }

        public Inventory GetBackpack()
        {
            if (!HasResources())
                return null;
            if (GUIController.PlayerCharacter == null)
            {
                Debug.LogError("Supposed to have all resources, but no linked game object", GUIController.PlayerCharacter);
                _backpack = null;
                return null;
            }

            if (_backpack != null)
            {
                return _backpack;
            }

            var allInventories = GUIController.PlayerCharacter.GetComponentsInChildren<Inventory>();
            ToolState toolState = GUIController.PlayerCharacter.GetComponent<ToolState>();
            Inventory mainInventory = null, toolbeltInventory = null;
            foreach (var i in allInventories)
            {
                if (i.isMain) mainInventory = i;
                else if (i.isToolbelt) toolbeltInventory = i;
            }

            return (_backpack = mainInventory);
        }


        // private void Update()
        // {
        //     if (GUIController.HasAllResources() == false)
        //     {
        //         _initialized = false;
        //         canvasGroup.alpha = 0;
        //         return;
        //     }
        //
        //     CheckInitialize();
        //     var inputPlayer = GUIController.playerInput;
        //     
        //     var recipeSelect = inputPlayer.actions["Select Recipe"].ReadValue<float>();
        //     if (recipeSelect != 0)
        //     {
        //         if(Time.realtimeSinceStartup - _timeLastUpdated > UpdateInterval)
        //             _timeLastUpdated = Time.realtimeSinceStartup;
        //         else
        //             return;
        //         if (recipeSelect > 0)
        //         {
        //             if(invertSelection)scrollView.SelectPrev();
        //             else scrollView.SelectNext();
        //         }
        //         else
        //         {
        //             if(invertSelection)scrollView.SelectNext();
        //             else scrollView.SelectPrev();
        //         }
        //     }
        // }
        //
        // private void CheckInitialize()
        // {
        //     if (_initialized == false)
        //     {
        //         _initialized = true;
        //         Initialize();
        //     }
        // }

        private void Initialize()
        {
            var backpack = GetBackpack();
            var toolState = ToolState;
            Debug.Assert(backpack != null && toolState != null, "Backpack or tool state is null",this);
            new ToolListener(this, toolState, backpack);
        }


        /// <summary>
        /// lifetime of this object should be until the player switches characters or the game ends
        /// </summary>
        public class ToolListener : IDisposable
        {
            private readonly ToolRecipeHUDController _toolRecipeHUDController;
            private readonly IDisposable _toolListener;
            private BoolReactiveProperty _isHudVisible;

            public bool IsVisible
            {
                get => _isHudVisible.Value;
                private set => _isHudVisible.Value = value;
            }
            public ToolListener(ToolRecipeHUDController toolRecipeHUDController, ToolState toolState, Inventory inventory)
            {
                _toolRecipeHUDController = toolRecipeHUDController;
                _isHudVisible = new BoolReactiveProperty(false);

                CompositeDisposable cd = new CompositeDisposable();

                toolState.EquippedToolRP
                    .StartWith(toolState.EquippedTool)
                    .Subscribe(tool =>
                    {
                        if (tool == null)
                        {
                            IsVisible = false;
                            return;
                        }

                        IsVisible = tool.UsesRecipes();
                        if (!IsVisible) return;
                        _toolRecipeHUDController.Setup(tool as Tool, inventory);
                        _toolRecipeHUDController.gameObject.SetActive(true);
                    }).AddTo(cd);

                _isHudVisible.Subscribe(visible => toolRecipeHUDController.canvasGroup.alpha = visible ? 1 : 0).AddTo(cd);
                _toolListener = cd;
            }

            public void Dispose()
            {
                _toolListener?.Dispose();
            }
        }
    }
}