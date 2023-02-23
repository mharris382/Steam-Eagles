using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI
{
    public class UIWheelTester : MonoBehaviour
    {
        public TestItem[] testItems;
        
        public UIWheelBuilder uiWheelBuilder;
        private IDisposable _openWheel;
        private List<UIWheelSelectable> _selectables;
        
        
        [Serializable]
        public class TestItem
        {
            public string name;
            public Sprite icon;
        }


        private void Awake()
        {
            _selectables = new List<UIWheelSelectable>(testItems.Select(t => new UIWheelSelectable()
            {
                isLocked = false,
                sprite = t.icon,
                name = t.name,
                onSelected = () => Debug.Log($"Selected {t.name}")
            }));
        }

        public void Update()
        {
            if(_openWheel == null && Input.GetKeyDown(KeyCode.Space))
            {
                _openWheel = uiWheelBuilder.CreateWheel(_selectables);
            }
            else if(_openWheel != null && Input.GetKeyDown(KeyCode.Space))
            {
                _openWheel.Dispose();
                _openWheel = null;
            }
        }
        
        
        
    }
}