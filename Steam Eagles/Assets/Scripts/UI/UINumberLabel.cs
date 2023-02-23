using System;
using TMPro;
using UnityEngine;

namespace UI
{
    [ExecuteInEditMode]
    [ExecuteAlways]
    public class UINumberLabel : MonoBehaviour
    {
        public TextMeshProUGUI text;
        public int startCount;
        public int endCount;

        bool HasResources()
        {
            if(text == null)
            {
                text = GetComponentInChildren<TextMeshProUGUI>();
            }

            return text != null;
        }

        private void OnEnable()
        {
            UpdateCountText();
        }

        private void OnTransformParentChanged()
        {
            UpdateCountText();
        }

        private void UpdateCountText()
        {
            if (HasResources())
            {
                int pos = transform.GetSiblingIndex();
                pos += startCount;
                pos %= endCount;
                text.text = pos.ToString();
            }
        }
    }
}