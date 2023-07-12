using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using Items.UI.HUDScrollView;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using Zenject;
using Object = UnityEngine.Object;

namespace Tools.UI
{
    public class CategoryUIFactory : PrefabFactory<List<Recipe>, HUDFancyScrollView>
    {
        public override HUDFancyScrollView Create(Object prefab, List<Recipe> original)
        {
            return base.Create(prefab, original);
        }
    }

    

    public class RecipeUIInstaller : MonoInstaller
    {
        [Required] public RecipeCategoryManager recipeCategoryManager;
        [Required] public RecipeCategoryUIController uiController;
        [Serializable]
        public class UIPrefabs
        {
            public HUDFancyScrollView recipeListPrefab;
            
        }
        [AssetList(AutoPopulate = true)] public Recipe[] recipes;
        public override void InstallBindings()
        {
            Container.Bind<IRecipeCategoryUIController>().To<RecipeCategoryUIController>().FromInstance(uiController)
                .AsSingle().NonLazy();
            
            Container.Bind<RecipeCategoryManager>().FromInstance(recipeCategoryManager).AsSingle().NonLazy();
            
            Container.Bind<ICategoryIcons>().FromMethod(GetCategoryIcons).AsSingle().NonLazy();
            
            Container.Bind<DiContainer>().FromInstance(Container).AsSingle()
                .WhenInjectedInto<RecipeCategoryManager.Controllers>().NonLazy();
            
            Container.Bind<RecipeCategoryManager.Controllers>().FromInstance(recipeCategoryManager.controllers).AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<RecipeUIInputs>().AsSingle().NonLazy();

            InputInstaller.Install(Container);
            //Container.Bind<ToolInputHelper>().AsSingle().NonLazy();

            Container.Bind<RecipeList>().FromInstance(new RecipeList(recipes)).AsSingle();
            Container.BindInterfacesAndSelfTo<CategoryList>().AsSingle().NonLazy();
        }
        
        ICategoryIcons GetCategoryIcons(InjectContext context)
        {
            var manager = context.Container.Resolve<RecipeCategoryManager>();
            return manager.categoryUI;
        }

        #region [Input Strategy Factory]

        class InputInstaller : Installer<InputInstaller>
        {
            public override void InstallBindings()
            {
                Container.BindFactory<KeyboardMouseInput, KeyboardMouseInput.Factory>().AsSingle().NonLazy();
                Container.BindFactory<GamepadInput, GamepadInput.Factory>().AsSingle().NonLazy();
                Container.BindFactory<bool, IInputWrapper, InputStrategyFactory>().FromFactory<CustomInputFactory>();
            }
        }

        class CustomInputFactory : IFactory<bool, IInputWrapper>
        {
            private readonly KeyboardMouseInput.Factory _kbmFactory;
            private readonly GamepadInput.Factory _gamepadFactory;

            public CustomInputFactory(KeyboardMouseInput.Factory kbmFactory, GamepadInput.Factory gamepadFactory)
            {
                _kbmFactory = kbmFactory;
                _gamepadFactory = gamepadFactory;
            }
            public IInputWrapper Create(bool param)
            {
                if (param)
                {
                    return _kbmFactory.Create();
                }
                else
                {
                    return _gamepadFactory.Create();
                }
            }
        }
        class InputWrapperFactoryImp : PlaceholderFactory<bool, IInputWrapper>, IFactory<bool, IInputWrapper>
        {
            [Inject] KeyboardMouseInput.Factory kbmFactory;
            [Inject] GamepadInput.Factory gamepadFactory;
            public IInputWrapper Create(bool param)
            {
                if(param) kbmFactory.Create();
                return gamepadFactory.Create();
            }
        }

        abstract class InputBase : IInputWrapper
        {
            [Inject]private ToolInputHelper _toolInputHelper;
            
            private PlayerInput PlayerInput => _toolInputHelper.HasInput() ? _toolInputHelper.PlayerInput : null;
            public bool WantsToDisassemble()
            {
                return PlayerInput.actions["Cancel"].IsPressed();
            }

            public float ReadCategorySelect()
            {
                if (_toolInputHelper.HasInput() == false) return 0;
                return PlayerInput.actions["Tool Select"].ReadValue<float>();
            }
            public float ReadRecipeSelect()
            {
                if (_toolInputHelper.HasInput() == false) return 0;
                return PlayerInput.actions["Recipe Select"].ReadValue<float>();
            }

            public bool CheckQuickSelectRecipe(Recipe recipe)
            {
                if (_toolInputHelper.HasInput() == false) return false;
                return CheckQuickSelectRecipe(PlayerInput, recipe);
            }

            public bool CheckQuickSelectCategory(string category)
            {
                if(_toolInputHelper.HasInput() == false) return false;
                return CheckQuickSelectCategory(PlayerInput, category);
            }

            public abstract bool CheckQuickSelectRecipe(PlayerInput input, Recipe recipe);
            public abstract bool CheckQuickSelectCategory(PlayerInput input, string category);


        }

        class KeyboardMouseInput : InputBase
        {
            public class Factory : PlaceholderFactory<KeyboardMouseInput>
            {
                 
            }
            public override bool CheckQuickSelectRecipe(PlayerInput playerInput, Recipe recipe)
            {
                return false;
            }

            public override bool CheckQuickSelectCategory(PlayerInput playerInput, string category)
            {
                return false;
            }
        }

        class GamepadInput : InputBase
        {
            public class Factory : PlaceholderFactory<GamepadInput>
            {
                 
            }
            public override bool CheckQuickSelectRecipe(PlayerInput playerInput, Recipe recipe)
            {
                return false;
            }
            public override bool CheckQuickSelectCategory(PlayerInput playerInput, string category)
            {
                return true;
            }
        }

        #endregion
        
        
        public interface ICategoryList
        {
            IList<string> Categories { get; }
        }
        public interface IRecipeList
        {
            public IList<Recipe> Recipes { get; }
        }
        
        public class CategoryList : ICategoryList
        {
            private readonly List<string> _categories = new();
            
            
            public IList<string> Categories => _categories.AsReadOnly();
            public CategoryList(Recipe[] recipes)
            {
                foreach (var recipe in recipes)
                {
                    if (_categories.Contains(recipe.category)) continue;
                    _categories.Add(recipe.category);
                }
            }
        }
        
        public class RecipeList 
        {
            private readonly List<Recipe> _recipes;

            public IList<Recipe> Recipes => _recipes.AsReadOnly();
            
            
            public RecipeList(Recipe[] recipes)
            {
                this._recipes = recipes.ToList();
            }
        }
    }



    public class InputStrategyFactory : PlaceholderFactory<bool, IInputWrapper> { }


    

    public interface IInputWrapper
    {
        bool WantsToDisassemble();
        float ReadCategorySelect( );
        float ReadRecipeSelect( );
        bool CheckQuickSelectRecipe( Recipe recipe);
        bool CheckQuickSelectCategory( string category);
    }
}