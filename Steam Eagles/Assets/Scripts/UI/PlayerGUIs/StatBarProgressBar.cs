using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UniRx;
using UnityEngine.UI;

namespace UI.PlayerGUIs
{
    [RequireComponent(typeof(StatBar))]
    public class StatBarProgressBar : MonoBehaviour
    {
        [Required] public Image imageFilled;
        [Required] public Image imageEmpty;
        private StatBar _statBar;
        public StatBar statBar => _statBar ? _statBar : _statBar = GetComponent<StatBar>();

        private void Awake()
        {
            statBar.OnValueChanged.Subscribe(_ => Redraw()).AddTo(this);
        }

        private void Redraw()
        {
            var rect = statBar.GetRect();
            float offset = 10;
            float thickness = 10;
            imageFilled.rectTransform.anchorMin = new Vector2(0, 0);
            imageFilled.rectTransform.anchorMax = new Vector2(0, 0);
            imageFilled.rectTransform.pivot = new Vector2(0, 0);
            imageFilled.rectTransform.anchoredPosition = new Vector2(rect.xMin, rect.yMin + offset);
            imageFilled.rectTransform.sizeDelta = new Vector2(rect.width * statBar.T, thickness);
            imageFilled.rectTransform.localPosition = new Vector3(0, -imageFilled.rectTransform.sizeDelta.y + offset);
            imageEmpty.rectTransform.anchorMin = new Vector2(0, 0);
            imageEmpty.rectTransform.anchorMax = new Vector2(0, 0);
            imageEmpty.rectTransform.pivot = new Vector2(0, 0); 
            imageEmpty.rectTransform.anchoredPosition = new Vector2(rect.xMin + rect.width * statBar.T, rect.yMin + offset);
            imageEmpty.rectTransform.sizeDelta = new Vector2(rect.width * (1 - statBar.T), thickness);
            imageEmpty.rectTransform.localPosition = new Vector3(imageFilled.rectTransform.sizeDelta.x, -imageEmpty.rectTransform.sizeDelta.y + offset);
        }
    }
}