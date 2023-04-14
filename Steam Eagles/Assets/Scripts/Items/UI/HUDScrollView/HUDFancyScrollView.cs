using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FancyScrollView;
using FancyEase = EasingCore.Ease;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Items.UI.HUDScrollView
{
    public class HUDFancyScrollView : FancyScrollView<Recipe, HUDToolRecipeContext>
    {
        [SerializeField] private Scroller scroller;
        [SerializeField, Required, AssetsOnly]
        private GameObject prefab;

        protected override GameObject CellPrefab => prefab;


        private Subject<int> _onSelectionChanged = new Subject<int>();

        public IObservable<int> OnSelectionChanged => _onSelectionChanged;

        
        protected override void Initialize()
        {
            base.Initialize();
            scroller.OnValueChanged(UpdatePosition);
            scroller.OnSelectionChanged(UpdateSelection);
        }

        public void SetContextInfo(Tool tool, Inventory inventory)
        {
            Debug.Assert(tool != null);
            Debug.Assert(inventory != null);
            
            this.Context.Tool = tool;
            this.Context.Inventory = inventory;
            UpdateContents(Context.Tool.Recipes.ToList());
        }

        public void SelectCell(int index)
        {
            if(index < 0 || index >= ItemsSource.Count || index == Context.SelectedIndex)
                return;
            
            UpdateSelection(index);
            scroller.ScrollTo(index, 0.34f, FancyEase.OutCubic);
        }

        void UpdateSelection(int index)
        {
            if (Context.SelectedIndex == index)
            {
                return;
            }

            Context.SelectedIndex = index;
            Refresh();
            _onSelectionChanged.OnNext(index);
        }
        
        
        public void SelectNext()
        {
            SelectCell(Context.SelectedIndex + 1);
        }
        public void SelectPrev()
        {
            SelectCell(Context.SelectedIndex - 1);
        }
        
        public void UpdateData(IList<Recipe> items)
        {
            UpdateContents(items);
            scroller.SetTotalCount(items.Count);
        }

    }
}