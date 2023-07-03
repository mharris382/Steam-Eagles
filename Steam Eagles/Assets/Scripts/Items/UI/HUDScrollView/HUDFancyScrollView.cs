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
    public abstract class RecipeCellGUI : MonoBehaviour
    {
        public void Assign(Inventory inventory)
        {
            
        }
    }
    public interface ICellGUI
    {   
        void SetContextInfo(Inventory inventory);
        void SetSelected(IObservable<Recipe> selectionStream);
    }


    public interface IValidityChecker
    {
        public bool IsRecipeValid(Recipe recipe, Inventory inventory);
    }

    public class HasIngredientsForRecipe : IValidityChecker
    {
        public bool IsRecipeValid(Recipe recipe, Inventory inventory) => recipe.IsAffordable(inventory);
    }

public class HUDFancyScrollView : FancyScrollView<Recipe, HUDToolRecipeContext>
    {
        [SerializeField] private Scroller scroller;
        [SerializeField, Required, AssetsOnly]
        private GameObject prefab;

        private IValidityChecker _validity;
        private Dictionary<GameObject, List<ICellGUI>> _cellGUIs = new Dictionary<GameObject, List<ICellGUI>>();

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
            if (index < 0 || index >= ItemsSource.Count || index == Context.SelectedIndex)
            {
                if(index == Context.SelectedIndex)
                    return;
                if (index < 0)
                {
                    index = ItemsSource.Count - 1;
                }
                else if (index == ItemsSource.Count)
                {
                    index = 0;
                }
            }
            
            UpdateSelection(index);
        }

        void UpdateSelection(int index)
        {
            if (Context.SelectedIndex == index)
            {
                return;
            }

            Context.SelectedIndex = index;
            scroller.ScrollTo(index, 0.34f, FancyEase.OutCubic);
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
        
        public void UpdateData(IList<Recipe> items, Inventory inventory, IObservable<Recipe> selectedRecipeStream)
        {
            _validity = new HasIngredientsForRecipe();
            
            scroller.SetTotalCount(items.Count);
            UpdateContents(items);
            scroller.SetTotalCount(items.Count);
            
        }
        public void UpdateData(IList<Recipe> items)
        {
            scroller.SetTotalCount(items.Count);
            UpdateContents(items);
            scroller.SetTotalCount(items.Count);
            
        }
        
        public void Select(Recipe recipe)
        {
            int index = ItemsSource.IndexOf(recipe);
            SelectCell(index);
        }
    }



}