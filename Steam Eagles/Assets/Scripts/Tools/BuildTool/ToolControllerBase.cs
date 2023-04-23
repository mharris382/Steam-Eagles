using System;
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
using UnityEngine.Events;

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
        private ToolActivator _activator;
        
        public Tool tool;
        private ToolAimHandler _aimHandler;

        protected ToolAimHandler AimHandler => _aimHandler ??= new ToolAimHandler(this, ToolState);
        protected CharacterState CharacterState => _characterState;
        public ToolState ToolState => CharacterState.Tool;
        protected Inventory Inventory => _inventory;
        protected EntityRoomState RoomState => _roomState;

        public ToolActivator Activator => _activator ??= new ToolActivator(this);
        

        [ShowInInspector, ReadOnly, PropertyOrder(-1)]
        public virtual string ToolMode
        {
            get;
            set;
        }


        public IIconable ToolIcon => tool as IIconable;


        private RecipeSelector _recipe;

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

        [Serializable]
        public class ActivationEvents
        {
            public UnityEvent onActivated;
            public UnityEvent onDeactivated;
            public UnityEvent<bool> onActivationStateChanged;
            public void OnActivationStateChanged(bool active)
            {
                onActivationStateChanged.Invoke(active);
                if (active)
                    onActivated.Invoke();
                else
                    onDeactivated.Invoke();
            }
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

        protected virtual void OnAwake(){}


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
        

        public virtual void StartLoading()
        {
            
        }
        public virtual bool IsDoneLoading()
        {
            return true;
        }

        /// <summary>
        /// completely finished loading and ready to go
        /// </summary>
        protected virtual void OnStart()
        {
        }


        private void OnDestroy()
        {
            _toolData.UnregisterTool(this);
        }

        protected abstract void OnRoomChanged(Room room);

        public void Activate(Tool tool)
        {
            this._toolData.AddTool(tool);
            //throw new NotImplementedException();
        }

        public abstract ToolStates GetToolState();
        
        public abstract bool UsesRecipes(out List<Recipe> recipes);

        public virtual void SetRecipeSelector(RecipeSelector recipeSelector)
        {
            Debug.Log($"{recipeSelector}");
        }


        private ToolModeListener _modeListener;
        protected virtual bool ToolUsesModes(out List<string> modes)
        {
            modes = null;
            return false;
        }
        
        /// <summary>
        /// tool is equipped and ready to be used
        /// </summary>
        public virtual void OnToolEquipped()
        {
            if (ToolUsesModes(out var modes))
            {
                if (_modeListener != null) _modeListener.Dispose();
                _modeListener = new ToolModeListener(modes, this);
                _modeListener.ListenForInput(ToolState.Inputs.OnToolModeChanged);
            }
        }

        /// <summary>
        /// tool is unequipped
        /// </summary>
        public virtual void OnToolUnEquipped()
        {
            if (_modeListener != null)
            {
                _modeListener.Dispose();
                _modeListener = null;
            }
        }

        
    }

    public class ToolModeListener
    {
        private IDisposable _disposable;
        private int _currentModeIndex;
        
        private readonly List<string> _modes;
        private readonly ToolControllerBase _controllerBase;
        private readonly IModeNameListener _ui;

        private string CurrentMode => _modes[_currentModeIndex];
        

        public ToolModeListener(List<string> modes, ToolControllerBase controllerBase)
        {
            _modes = modes;
            _controllerBase = controllerBase;
            if (controllerBase.modeDisplayUI != null)
            {
                _ui = controllerBase.modeDisplayUI.GetComponent<IModeNameListener>();
            }
        }

        public void ListenForInput(Subject<Unit> onToolModeChanged)
        {
            _disposable = onToolModeChanged.StartWith(Unit.Default).Subscribe(_ => OnModeChanged());
        }

        void OnModeChanged()
        {
            if(_currentModeIndex == _modes.Count - 1)
                _currentModeIndex = 0;
            else
                _currentModeIndex++;
            _controllerBase.ToolMode = CurrentMode;
            if (_ui != null)
                _ui.DisplayModeName(CurrentMode);
        }

        public void Dispose()
        {
            _disposable?.Dispose();
            _disposable = null;
        }
    }
    
    
    public class ToolActivator
    {
        private readonly ToolControllerBase _controllerBase;
        private IDisposable _disposable;
        private BoolReactiveProperty _isEquipped = new BoolReactiveProperty();
        private BoolReactiveProperty _isInUse = new BoolReactiveProperty();
        private BoolReactiveProperty _isActive = new BoolReactiveProperty();

        public bool IsInUse
        {
            set => _isInUse.Value = value;
        }

        public bool IsEquipped
        {
            set => _isEquipped.Value = value;
        }
            
        public IReadOnlyReactiveProperty<bool> IsActive => _isActive;

        public ToolActivator(ToolControllerBase controllerBase)
        {
            _controllerBase = controllerBase;
            _isEquipped = new BoolReactiveProperty();
            _isInUse = new BoolReactiveProperty();
            _isActive = new BoolReactiveProperty();
            var cd = new CompositeDisposable();
            _isEquipped.Select(t => (t, _isInUse.Value)).Subscribe(t => OnStateChanged(t.t, t.Value)).AddTo(cd);
            _isInUse.Select(t => (_isEquipped.Value, t)).Subscribe(t => OnStateChanged(t.Item1, t.t)).AddTo(cd);
            _disposable = cd;
        }

        void OnStateChanged(bool equipped, bool inUse) => _isActive.Value = equipped && inUse;

        public void Dispose()
        {
            if (_disposable != null)
            {
                _disposable.Dispose();
                _disposable = null;
            }
        }
    }

}