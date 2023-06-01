using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace CoreLib.Entities
{
    [System.Obsolete("Use EntityHandle + EntityLinkRegistry instead")]
    [DisallowMultipleComponent]
    public class Entity : MonoBehaviour
    {
        public EntityType entityType = EntityType.CHARACTER;
        public string entityGUID;
        public bool dynamic = false;
        
        private ReactiveProperty<GameObject> _linkedGameObject = new ReactiveProperty<GameObject>();

        
        #region [Properties]

        private ReactiveProperty<GameObject> RxLinkedGameObjectProperty => _linkedGameObject ??= new ReactiveProperty<GameObject>();
        [ShowInInspector, HideInEditorMode, ReadOnly]
        public GameObject LinkedGameObject
        {
            set
            {
                if (RxLinkedGameObjectProperty.Value == null)
                {
                    RxLinkedGameObjectProperty.Value = value;
                    return;
                }

                throw new Exception();
            }
            get => RxLinkedGameObjectProperty.Value;
        }

        public IReadOnlyReactiveProperty<GameObject> LinkedGameObjectProperty => RxLinkedGameObjectProperty;

        

        #endregion
    }
}