using Items.UI.HUDScrollView;
using Sirenix.OdinInspector;
using UnityEngine;
using FancyScrollView;
using System;
using System.Collections;
using UniRx;

namespace Tools.UI
{
    public class RecipeCategoriesScrollView : FancyScrollView<string, RecipeCategoriesContext>
    {
        [Required]
        public GameObject prefab;

        protected override GameObject CellPrefab => prefab;
        
        
        public void Setup(IRecipeCategoryUIController controller)
        {
            this.Context.Controller = controller;
        }
    }
    
    public class RecipeCategoriesContext
    {
        
        public IRecipeCategoryUIController Controller { get; set; }
        public bool ReadyToUse => Controller != null && Controller.IsInitialized;

        private Subject<Unit> _onReady = new();

        private Coroutine _coroutine;
        public void WaitForReady(MonoBehaviour caller, Action onReady)
        {
            _onReady.Subscribe(_ => onReady());
            if (ReadyToUse)
            {
                _onReady.OnNext(Unit.Default);
            }
            else if(_coroutine != null)
            {
                _coroutine = caller.StartCoroutine(WaitForReadyCoroutine());
            }
        }
        IEnumerator WaitForReadyCoroutine()
        {
            while (!ReadyToUse)
            {
                yield return null;
            }

            _coroutine = null;
            _onReady.OnNext(Unit.Default);
            _onReady.OnCompleted();
        }
    }
}