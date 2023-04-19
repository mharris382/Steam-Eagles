using System;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
{
    [RequireComponent(typeof(Button))]
    public class SettingsGroupButton : MonoBehaviour
    {
        [SerializeField,Required]
        private SettingGroup groupPanel;
        
        [SerializeField,Required]
        private Image panelOpenIcon;
        
        private Button _button;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            panelOpenIcon.enabled = false;
            
            
        }

        private void Start()
        {
            groupPanel.GroupOpened.Subscribe(_ =>
            {
                _button.interactable = false;
                panelOpenIcon.enabled = true;
            }).AddTo(this);
            
            groupPanel.GroupClosed.Subscribe(_ =>
            {
                _button.interactable = true;
                panelOpenIcon.enabled = false;
            }).AddTo(this);
            
            _button.onClick.AsObservable().Subscribe(_ =>
            {
                var panelParent = groupPanel.transform.parent;
                for (int i = 0; i < panelParent.childCount; i++)
                {
                    var child = panelParent.GetChild(i);
                    child.gameObject.SetActive(false);
                }
                groupPanel.gameObject.SetActive(true);
            }).AddTo(this);
        }
    }
}