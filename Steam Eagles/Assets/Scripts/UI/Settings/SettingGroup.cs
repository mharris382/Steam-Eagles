using System;
using UniRx;
using UnityEngine;

namespace UI.Settings
{
    public class SettingGroup : MonoBehaviour
    {
        public Subject<Unit> GroupOpened = new Subject<Unit>();
        public Subject<Unit> GroupClosed = new Subject<Unit>();
        public void OnEnable()
        {
            GroupOpened.OnNext(Unit.Default);
        }

        public void OnDisable()
        {
            GroupClosed.OnNext(Unit.Default);
        }
    }
}