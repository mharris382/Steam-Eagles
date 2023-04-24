using System.Collections;
using System.Collections.Generic;
using Buildings;
using Buildings.Rooms;
using Buildings.Rooms.Tracking;
using Characters;
using CoreLib;
using Items;
using Sirenix.OdinInspector;
using SteamEagles.Characters;
using Tools.RecipeTool;
using UniRx;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Tools.BuildTool
{
    public abstract class ToolControllerBase : MonoBehaviour
    {
        
        [SerializeField] private ActivationEvents activationEvents;
        [SerializeField,ValidateInput(nameof(ValidateUI))] public GameObject modeDisplayUI;

        bool ValidateUI(GameObject mode, ref string error)
        {
            if (mode == null)
            {
                return true;
            }

            var displayUI = mode.GetComponent<IModeNameListener>();
            if (displayUI == null)
            {
                error = $"{mode.name} must implement {nameof(IModeNameListener)}";  
                return false;
            }
            return true;
        }
        
        
        public Building targetBuilding;
        private CharacterState _characterState;
        private Inventory _inventory;
        private EntityRoomState _roomState;
        private BoolReactiveProperty _hasRoom = new BoolReactiveProperty(false);
        private ToolControllerSharedData _toolData;
        private bool _isActive;

        public Tool tool;


        private ToolActivator _activator;
        private RecipeSelector _recipe;
        private ToolAimHandler _aimHandler;
        private ToolModeListener _modeListener;

        protected ToolAimHandler AimHandler => _aimHandler ??= new ToolAimHandler(this, ToolState);
        protected CharacterState CharacterState => _characterState;
        public ToolState ToolState => CharacterState.Tool;
        protected Inventory Inventory => _inventory;
        protected EntityRoomState RoomState => _roomState;

        public ToolControllerSharedData SharedData => _toolData;

        public ToolActivator Activator => _activator ??= new ToolActivator(this);


        [ShowInInspector, ReadOnly, PropertyOrder(-1)]
        public virtual string ToolMode
        {
            get;
            set;
        }

        public IIconable ToolIcon => tool as IIconable;


        public RecipeSelector Recipe
        {
            get
            {
                if (_recipe == null && UsesRecipes(out var recipes))
                {
                    _recipe = new RecipeSelector(recipes);
                    SetRecipeSelector(_recipe);
                }
                return _recipe;
            }
        }

        public bool HasRoom
        {
            get => _hasRoom.Value;
            protected set => _hasRoom.Value = value;
        }

        

        public void SetActive(bool isActive)
        {
            if (isActive != _isActive)
            {
                _isActive = isActive;
                activationEvents.OnActivationStateChanged(isActive);
            }
        }

        private void Awake()
        {
            _isActive = false;
            _characterState = GetComponentInParent<CharacterState>();
            var inputState = _characterState.GetComponentInChildren<CharacterInputState>();
            _toolData = GetComponentInParent<ToolControllerSharedData>();

            Debug.Assert(_toolData != null, "ToolControllerSharedData not found", this);
            _toolData.RegisterTool(this);
            if(!_characterState.gameObject.TryGetComponent(out _roomState))
                _roomState = _characterState.gameObject.AddComponent<EntityRoomState>();
            
            var allInventories = _characterState.GetComponentsInChildren<Inventory>();
            foreach (var inventory in allInventories)
            {
                if (inventory.isMain)
                {
                    _inventory = inventory;
                    break;
                }
            }

            if (_inventory == null)
                Debug.LogError($"Couldn't find main inventory on character{_characterState.name}", _characterState);
            _hasRoom = new BoolReactiveProperty(false);
            OnAwake();
        }


        public bool HasResources()
        {
            if (targetBuilding == null)
            {
                targetBuilding = GetComponentInParent<Building>();
                if (targetBuilding == null) targetBuilding = FindObjectOfType<Building>();
            }
            return _inventory != null && _characterState != null && _roomState != null;
        }

        private IEnumerator Start()
        {
            
            while (!_characterState.IsEntityInitialized)
                yield return null;
            
            StartLoading();
            while (!IsDoneLoading())
                yield return null;
            
            if (_toolData.ActiveTool.Value == null)
            {
                _toolData.ActiveTool.Value = this;
            }
           
            _roomState.CurrentRoom.Subscribe(OnRoomChanged).AddTo(this);
            _roomState.CurrentRoom.Subscribe(room =>
            {
                if (room != null)
                {
                    targetBuilding = room.GetComponentInParent<Building>();
                }
            }).AddTo(this);
            OnRoomChanged(_roomState.CurrentRoom.Value);
            OnStart();
        }


        private void OnDestroy()
        {
            _toolData.UnregisterTool(this);
        }


        public void Activate(Tool tool)
        {
            this._toolData.AddTool(tool);
        }


        public void SetToolEquipped(bool equipped)
        {
            void UpdateToolModes(bool equip)
            {
                if (equip)
                {
                    if (ToolUsesModes(out var modes))
                    {
                        if (_modeListener != null) _modeListener.Dispose();
                        _modeListener = new ToolModeListener(modes, this);
                        _modeListener.ListenForInput(ToolState.Inputs.OnToolModeChanged);
                    }
                }
                else if (_modeListener != null)
                {
                    _modeListener.Dispose();
                    _modeListener = null;
                }
            }
            Activator.IsEquipped = equipped;
            UpdateToolModes(equipped);
            if (equipped)
            {
                OnToolEquipped();
            }
            else
            {
                OnToolUnEquipped();
            }
        }


        /// <summary>
        /// specify the tool slot that this tool is assigned to
        /// </summary>
        /// <returns></returns>
        public abstract ToolStates GetToolState();


        /// <summary>
        /// called whenever the character enters a new room.
        /// Use this method to disable tool functionality if the room is not valid for that tool
        /// </summary>
        /// <param name="room"></param>
        protected virtual void OnRoomChanged(Room room) { }


        /// <summary>
        /// use this to begin any async loading operations that need to be done before the tool is ready to go
        /// be sure to implement <see cref="IsDoneLoading"/> to tell the tool controller when loading is done
        /// </summary>
        public virtual void StartLoading() { }

        /// <summary>
        /// returns true if loading is done.  use this to tell tool controller when the async loading is done.
        /// load operations should be started in <see cref="StartLoading"/>
        /// </summary>
        /// <returns></returns>
        public virtual bool IsDoneLoading() => true;


        /// <summary>
        /// called inside awake
        /// </summary>
        protected virtual void OnAwake(){}

        /// <summary>
        /// completely finished loading and ready to go
        /// </summary>
        protected virtual void OnStart() { }

        public abstract BuildingLayers GetTargetLayer();
        

        /// <summary>
        /// if the tool uses recipes, return true and set the recipes list. This will enable the recipe selector
        /// and trigger a call to <see cref="SetRecipeSelector"/>
        /// </summary>
        /// <param name="recipes"></param>
        /// <returns></returns>
        public abstract bool UsesRecipes(out List<Recipe> recipes);

        /// <summary>
        /// if the tool uses recipes this will be called once, when the recipe selector is created
        /// </summary>
        /// <param name="recipeSelector"></param>
        public virtual void SetRecipeSelector(RecipeSelector recipeSelector) { }

        
        
        

        /// <summary>
        /// called each time tool is equipped
        /// </summary>
        public virtual void OnToolEquipped() { }

        /// <summary>
        /// called each time the tool is unequipped
        /// </summary>
        public virtual void OnToolUnEquipped() { }
        
        
        

        /// <summary>
        /// implement this method if the tool has operation modes.
        /// Do not return true if modes list is empty
        /// </summary>
        /// <param name="modes"></param>
        /// <returns>true if tool has modes, otherwise false</returns>
        protected virtual bool ToolUsesModes(out List<string> modes)
        {
            modes = null;
            return false;
        }
    }
}