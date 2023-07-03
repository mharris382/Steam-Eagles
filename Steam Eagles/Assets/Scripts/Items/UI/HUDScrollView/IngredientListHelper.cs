using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Items.UI.HUDScrollView
{
    
    /// <summary>
    /// animates the preferred width of the child LayoutElements, to be used with a content size fitter
    /// </summary>
    public class IngredientListHelper : MonoBehaviour
    {
        [OnValueChanged(nameof(UpdateValues))]

        [ValidateInput(nameof(IsValid), "Need a LayoutElement in child")]
        public float widthPerItem = 50f;
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        private LayoutElement[] _layoutElements;
        [RangeReactiveProperty(0,1)]
        public FloatReactiveProperty percentWidth = new FloatReactiveProperty(1f);


        private LayoutElement _layoutElement;
        private LayoutElement LayoutElement
        {
            get
            {
                if (_layoutElement == null)
                {
                    _layoutElement = GetComponent<LayoutElement>();
                }
                return _layoutElement;
            }
        }
        bool IsValid(float w){
            
            _layoutElements = GetComponentsInChildren<LayoutElement>();
            return _layoutElements != null && _layoutElements.Length > 0;
        }
        private void Awake()
        {
            _layoutElements = GetComponentsInParent<LayoutElement>();
        }

        void UpdateValues(float w)
        {
            _layoutElements = GetComponentsInChildren<LayoutElement>();
            SetWidth(percentWidth.Value);
        }
        public void SetWidth(float percentWidth)
        {
            if (!Application.isPlaying || _layoutElements == null)
            {
                _layoutElements = GetComponentsInChildren<LayoutElement>();
            }
            
            float percent = curve.Evaluate(percentWidth);
            float width = widthPerItem * percent;
            LayoutElement.preferredWidth = width * transform.childCount;
            
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                return;
            }
            if (percentWidth == null) percentWidth = new(1f);
            SetWidth(percentWidth.Value);
            
        }
    }
}