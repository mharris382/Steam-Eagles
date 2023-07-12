using System;
using System.Collections;
using System.Linq;
using Items;
using UniRx;
using UnityEngine;
using Zenject;

namespace Tools.UI
{
    public interface IRecipeUIInputs
    {
        int CategoryCycleSelect { get; set; }
        int RecipeCycleSelect { get; set; }
        void QuickSelect(string category);
        void QuickSelect(Recipe recipe);
    }

    

    public class RecipeUIInputs : IInitializable, IDisposable, ITickable, IRecipeUIInputs
    {
   
        public int CategoryCycleSelect { get; set; }
        public int RecipeCycleSelect { get; set; }


        private Subject<int> _categoryCycleSelect = new();
        private Subject<int> _recipeCycleSelect = new();
        private Subject<string> _quickSelectCategory = new();
        private Subject<Recipe> _quickSelectRecipe = new();

        
        private float _cycleSelectDelay = 0.1f;

        [Inject]
        private CoroutineCaller _coroutineCaller;
        [Inject]
        private IRecipeCategoryUIController _controller;

        [Inject]
        private ToolInputHelper _toolInputHelper;

        [Inject]
        private InputStrategyFactory _inputStrategyFactory;

        private IInputWrapper _inputWrapper;
        
        private bool _isFullyInitialized = false;
        private BoolReactiveProperty _isKbm = new();

        private CompositeDisposable _cd = new();
        private float CycleSelectDelay => _cycleSelectDelay <= 0 ? 0.1f : _cycleSelectDelay;
        public IObservable<string> OnQuickSelectCategory => _quickSelectCategory;
        public IObservable<Recipe> OnQuickSelectRecipe => _quickSelectRecipe;
        
        

        private IInputWrapper InputWrapper
        {
            get
            {
                if (_inputWrapper == null) _inputWrapper = _inputStrategyFactory.Create(_isKbm.Value);
                return _inputWrapper;
            }
        }
        
        public void QuickSelect(string category)
        {
            _quickSelectCategory.OnNext(category);
        }

        public void QuickSelect(Recipe recipe)
        {
            if (recipe == null) return;
            _quickSelectRecipe.OnNext(recipe);
        }

      
        public void Initialize()
        {
            _coroutineCaller.StartCoroutine(WaitForReady());
        }

        IEnumerator WaitForReady()
        {
            while (_controller == null)
            {
                yield return null;
            }
            while(!_controller.IsInitialized)
            {
                yield return null;
            }
            Setup();
        }

        void Setup()
        {
            _quickSelectRecipe.Select(t => t.category).Subscribe(_quickSelectCategory);
            
            var categoryChangeStream = _categoryCycleSelect.Buffer(TimeSpan.FromSeconds(CycleSelectDelay)).Where(t => t.Count > 0)
                .Select(t => t.FirstOrDefault(x => x != 0));
            
            var recipeChangeStream = _recipeCycleSelect.Buffer(TimeSpan.FromSeconds(CycleSelectDelay)).Where(t => t.Count > 0)
                .Select(t => t.FirstOrDefault(x => x != 0));

            recipeChangeStream.Where(t => t > 0).Subscribe(_ => _controller.SelectNextRecipe()).AddTo(_cd);
            recipeChangeStream.Where(t => t < 0).Subscribe(_ => _controller.SelectPreviousRecipe()).AddTo(_cd);
            categoryChangeStream.Where(t => t > 0).Subscribe(_ => _controller.SelectNextCategory()).AddTo(_cd);
            categoryChangeStream.Where(t => t < 0).Subscribe(_ => _controller.SelectPreviousCategory()).AddTo(_cd);
            _isKbm.Subscribe(t => _inputWrapper = _inputStrategyFactory.Create(t)).AddTo(_cd);
            _isFullyInitialized = true;

        }

        public void Dispose()
        {
            _quickSelectCategory.Dispose();
            _categoryCycleSelect.Dispose();
            _recipeCycleSelect.Dispose();
            _cd.Dispose();
            _quickSelectRecipe.Dispose();
        }

        public void Tick()
        {
            if(!_isFullyInitialized) return;
            if (_toolInputHelper.HasInput() == false) return;
            _isKbm.Value = _toolInputHelper.IsUsingKeyboardMouse();
            var selectCat = _inputWrapper.ReadCategorySelect();
            if (selectCat != 0)
            {
                CategoryCycleSelect = selectCat > 0 ? 1 : -1;    
            }
            else
            {
                CategoryCycleSelect = 0;
            }
            
            var selectRec  = InputWrapper.ReadRecipeSelect();
            if (selectRec != 0)
            {
                RecipeCycleSelect = selectRec > 0 ? 1 : -1;    
            }
            else
            {
                RecipeCycleSelect = 0;
            }
             
            _categoryCycleSelect.OnNext(CategoryCycleSelect);
            _recipeCycleSelect.OnNext(RecipeCycleSelect);

            foreach (var category in _controller.GetCategories())
            {
                if (InputWrapper.CheckQuickSelectCategory(category) == true)
                {
                    QuickSelect(category);
                    return;
                }
                foreach (var reccipe in _controller.GetRecipes(category))
                {
                    if (InputWrapper.CheckQuickSelectRecipe(reccipe) == true)
                    {
                        QuickSelect(reccipe);
                        return;
                    }
                }
            }

          
        }
    }
}