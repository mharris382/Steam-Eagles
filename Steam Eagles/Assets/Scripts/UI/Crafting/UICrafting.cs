using System;
using System.Collections.Generic;
using Buildables;
using Buildings;
using Buildings.Rooms.Tracking;
using Buildings.Tiles;
using CoreLib;
using CoreLib.SharedVariables;
using Items;
using Sirenix.OdinInspector;
using UI;
using UI.Crafting;
using UI.Crafting.Destruction;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Zenject;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UICrafting : UIPlayerSystemBase
{
    public Recipes recipes;
    public InputActions inputActions;
    public Shortcuts shortcuts;
    public UICraftingMode craftingMode;
    public HoverPosition hoverPosition;
    
    
    
    [Serializable]
    public class InputActions
    {
        public InputActionReference nextCategory;
        public InputActionReference prevCategory;
        public InputActionReference nextRecipe;
        public InputActionReference prevRecipe;

        public void Process(PlayerInput playerInput, Recipes recipes)
        {
            if(playerInput.actions["Next Category"].WasPerformedThisFrame()) recipes.NextCategory();
            if(playerInput.actions["Previous Category"].WasPerformedThisFrame()) recipes.PrevCategory();
            if(playerInput.actions["Next Recipe"].WasPerformedThisFrame()) recipes.NextRecipe();
            if(playerInput.actions["Previous Recipe"].WasPerformedThisFrame()) recipes.PrevRecipe();
        }
    }
    
    
    [Serializable]
    public class Shortcuts
    {
        public string[] shortcutCategories = new[]
        {
            "Shortcut - 1",
            "Shortcut - 2",
            "Shortcut - 3",
            "Shortcut - 4"
        };
        public void ProcessShortcuts(PlayerInput playerInput, Recipes recipes)
        {
            if (playerInput.currentControlScheme.Contains("Keyboard") == false) return;
            try
            {
                for (int i = 0; i < shortcutCategories.Length; i++)
                {
                    if (playerInput.actions[shortcutCategories[i]].WasPerformedThisFrame())
                    {
                        recipes.SelectCategory(i);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            
        }
    }
    
    
    
    private PlayerInput _playerInput;
    private GameObject _character;
    private Camera _camera;
    private CraftingAimHanding _aimHanding;
    private CraftingDirectionHandler _directionHandler;
    private LoadHelper _recipeLoader;
    private EntityRoomState _roomState;
    private RecipePreviewController _previewController;
    private PlacementValidity _placementValidity;
    private DestructionPreviewController _destructionPreviewController;

    public bool IsReady => _playerInput && _character && _camera;
    
    public bool IsCurrentRecipeLoaded { get; set; }
    
    private Subject<Unit> _onGameStarted = new Subject<Unit>();

    [ShowInInspector, BoxGroup("Debug")] public string CurrentCategory => recipes.CurrentCategoryName?.Value;
    [ShowInInspector, BoxGroup("Debug")] public Recipe CurrentRecipe => recipes.CurrentRecipe?.Value;
    [ShowInInspector, BoxGroup("Debug")] public List<Recipe> CurrentRecipes => recipes.CurrentRecipes?.Value;
    
    public IObservable<Unit> GameStarted => _onGameStarted;
    protected override void OnPlayerJoined(PlayerInput playerInput)
    {
        _playerInput = playerInput;
    }

    protected override void OnGameStarted(PlayerInput playerInput, GameObject character, Camera camera)
    {
        this._character = character;
        this._roomState = _character.GetComponent<EntityRoomState>();
        _camera = camera;
        _playerInput = playerInput;
        _onGameStarted.OnNext(Unit.Default);
    }


    [Inject] public void Install(LoadHelper loadHelper, CraftingAimHanding aimHanding, PlacementValidity placementValidity,
        RecipePreviewController previewController, DestructionPreviewController destructionPreviewController, CraftingDirectionHandler directionHandler)
    {
        _directionHandler = directionHandler;
        _recipeLoader = loadHelper;
        _aimHanding = aimHanding;
        _placementValidity = placementValidity;
        _previewController = previewController;
        _destructionPreviewController = destructionPreviewController;
    }

    private void Awake()
    {
        MessageBroker.Default.Receive<CharacterAssignedPlayerInputInfo>().Subscribe(OnCharacterAssignedPlayerInput)
            .AddTo(this);
        recipes.Init();
       //_recipeLoader = new LoadHelper(recipes, this);
       //_aimHanding = new CraftingAimHanding();
       //_placementValidity = new PlacementValidity(this);
       //_previewController = new RecipePreviewController();
       //_destructionPreviewController = new DestructionPreviewController();
    }

    void OnCharacterAssignedPlayerInput(CharacterAssignedPlayerInputInfo info)
    {
        var camera = info.camera.GetComponent<Camera>();
        var input = info.inputGo.GetComponent<PlayerInput>();
        var character = info.characterState.gameObject;
        OnGameStarted(input, character, camera);
    }
    void Update()
    {
        
    }

    public void UpdateCrafting()
    {
        if(!IsReady) return;
        
        
        
        inputActions.Process(_playerInput, recipes);
        shortcuts.ProcessShortcuts(_playerInput, recipes);
        craftingMode.Process(_playerInput);
        if(_roomState.CurrentRoom.Value == null) return;
        ProcessCraftingUI();
    }

    private void ProcessCraftingUI()
    {
        if (!_recipeLoader.IsCurrentRecipeLoaded) return;
        var recipe = recipes.CurrentRecipe.Value;
        var loadedObject = _recipeLoader.CurrentRecipeLoadedObject;
        var targetLayer = _recipeLoader.CurrentTargetLayer;
        if (targetLayer == BuildingLayers.NONE) return;
        var building = _roomState.CurrentRoom.Value.Building;
        var gridPosition = _aimHanding.ProcessGridAim(_playerInput, _character, _camera, building, targetLayer);
        
        _directionHandler.UpdateDirection(_playerInput);
        if (craftingMode.isDestructMode.Value)
        {
            if(_destructionPreviewController == null) return;
            _destructionPreviewController.UpdateDestructTarget(recipe, _playerInput, building, gridPosition);
        }
        else
        {
            _placementValidity.UpdateValidity(recipe, loadedObject, _character, building, gridPosition);
            _previewController.UpdatePreview(recipe, loadedObject, _playerInput, building, gridPosition, _placementValidity.IsValid);
        
            if (_placementValidity.IsValid)
            {
                if (_playerInput.actions["Ability Primary"].IsPressed())
                {
                    _previewController.BuildFromPreview( building, gridPosition);
                }
            }
        }
        
    }
}


public static class RecipeExtensions
{
    public static BuildingLayers GetBuildingLayer(this Recipe recipe, Object loadedObject)
    {
        switch (recipe.GetRecipeType())
        {
            case Recipe.RecipeType.TILE:
                return GetBuildingLayerFromTile(loadedObject as TileBase);
                break;
            case Recipe.RecipeType.MACHINE:
                return GetBuildingLayerFromPrefab(loadedObject as GameObject);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        static BuildingLayers GetBuildingLayerFromTile(TileBase tileBase)
        {
            if (tileBase is EditableTile eTile)
            {
                return eTile.GetLayer();
            }
            return BuildingLayers.SOLID;
        }
        static BuildingLayers GetBuildingLayerFromPrefab(GameObject prefab)
        {
            var machine = prefab.GetComponent<BuildableMachine>();
            if (machine)
            {
                return machine.GetTargetLayer();
            }
            return BuildingLayers.SOLID;
        }
    }
}