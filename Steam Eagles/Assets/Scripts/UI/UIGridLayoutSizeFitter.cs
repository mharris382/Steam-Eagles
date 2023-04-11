using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(LayoutElement) ,typeof(GridLayoutGroup))]
    public class UIGridLayoutSizeFitter : MonoBehaviour
    {
        [OnValueChanged(nameof(UpdateLayout))]
        public int rows = 5;
        
        [OnValueChanged(nameof(UpdateLayout))]
        public int columns = 4;
        
        [OnValueChanged(nameof(UpdateLayout))]
        [MinMaxRange(10, 200)]
        public Vector2Int cellSizeRange = new Vector2Int(50, 100);


        private GridLayoutGroup _gridLayout;
        public GridLayoutGroup GridLayout => _gridLayout ? _gridLayout : _gridLayout = GetComponent<GridLayoutGroup>();

        private LayoutElement _layoutElement;
        public LayoutElement LayoutElement => _layoutElement ? _layoutElement : _layoutElement = GetComponent<LayoutElement>();

        private RectTransform _rectTransform;
        public RectTransform RectTransform => _rectTransform ? _rectTransform : _rectTransform = GetComponent<RectTransform>();

        void UpdateLayout()
        {
            var preferredSize = GridLayout.cellSize.y;
            var minSize = GridLayout.cellSize.x;
            
            LayoutElement.preferredWidth = (preferredSize + GridLayout.spacing.x) * columns + GridLayout.padding.horizontal;
            LayoutElement.preferredHeight = (preferredSize + GridLayout.spacing.y) * rows + GridLayout.padding.vertical;

            LayoutElement.minWidth = (minSize + GridLayout.spacing.x) * columns + GridLayout.padding.horizontal;
            LayoutElement.minHeight = (minSize + GridLayout.spacing.y) * rows + GridLayout.padding.vertical;
            
            GridLayout.constraintCount = columns;
            GridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            GridLayout.cellSize = new Vector2(preferredSize, preferredSize);
        }
    }
}