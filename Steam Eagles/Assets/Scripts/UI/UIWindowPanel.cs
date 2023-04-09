using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIWindowPanel : MonoBehaviour
    {
        [Serializable]
        private class WindowPanel
        {
            public Window window;
            public Button button;

            public WindowPanel(Window window, Button button)
            {
                this.window = window;
                this.button = button;
            }
        }

        [TableList(AlwaysExpanded = true)]
        [SerializeField] private WindowPanel[] panels;
        
        [FoldoutGroup("Auto Generate")]
        [SerializeField] private AutoWindowPanel autoWindow;


        [FoldoutGroup("Auto Generate")]
        [Button]
        void AutoGenerate()
        {
            panels = autoWindow.GetWindows().ToArray();
        }
        
        [Serializable]
        private class AutoWindowPanel
        {
            public Transform buttonParent;
            public Transform windowParent;

            public AutoWindowPanel(Transform buttonParent, Transform windowParent)
            {
                this.buttonParent = buttonParent;
                this.windowParent = windowParent;
            }

            public IEnumerable<WindowPanel> GetWindows()
            {
                for (int i = 0; i < buttonParent.childCount; i++)
                {
                    var button = buttonParent.GetChild(i).GetComponent<Button>();
                    var buttonName = buttonParent.GetChild(i).name;
                    var window = windowParent.Find(buttonName).GetComponent<Window>();
                    if(window==null||button==null)
                        continue;
                    yield return new WindowPanel(window, button);
                }
            }
        }


        public void OnSelected(GameObject go)
        {
            var panel = panels.FirstOrDefault(t => t.button.gameObject == go);
            if (panel == null)
                return;
            SwitchTo(panel);
        }

        private void Awake()
        {
            foreach (var panel in panels)
            {
                if (panel.button == null || panel.window == null) 
                {
                    Debug.LogError($"Missing Button or Window on {this.name}: Window={panel.window}, Panel={panel.window}",this);
                    continue;
                }
                panel.button.onClick.AsObservable().Select(t => panel).Subscribe(SwitchTo).AddTo(this);
                panel.window.Close();
            }
        }
        
        void SwitchTo(WindowPanel panel)
        {
            foreach (var windowPanel in panels)
            {
                windowPanel.window.IsVisible = windowPanel == panel;
                windowPanel.button.interactable = windowPanel != panel;
            }
        }
    }
}