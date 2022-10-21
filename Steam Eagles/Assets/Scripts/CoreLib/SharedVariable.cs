using System;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CoreLib
{
    public abstract class SharedVariable<T> : ScriptableObject where T : class
    {
        
        [SerializeField] private T value;
        [SerializeField] private bool debugVariable;
        
        public UnityEvent<T> onValueChanged;
        private bool _hasValue;
        
        public virtual T Value
        {
            get => value;
            set
            {
                if (value != this.value)
                {
                    
                    this.value = value;
                    onValueChanged?.Invoke(this.value);
                    if (debugVariable) Debug.Log($"SHARED VARIABLE:{name} changed (new value={value}", this);
                }
                _hasValue = this.value != null;
            }
        }

        public bool HasValue
        {
            get
            {
                if (value is Object o)
                {
                    return o != null;
                }
                return _hasValue;
            }
        }

        private void Awake()
        {
            Value = value;//this ensures that _hasValue is accurate
        }

        private void OnEnable()
        {
            Value = value;//this ensures that _hasValue is accurate
        }
    }

    public abstract class SharedArray<T> : SharedVariable<T[]> where T : class
    {
        
    }

    public abstract class SharedVariableEvents<T, TShared> : MonoBehaviour
        where T : UnityEngine.Object
        where TShared : SharedVariable<T>
    {
        [SerializeField] private TShared sharedVariable;
        
        
        public UnityEvent<T> onValueChanged;

        private void OnEnable()
        {
            sharedVariable.onValueChanged.AddListener(OnVariableChanged);
            if (sharedVariable.Value != null) OnVariableChanged(sharedVariable.Value);
        }

        private void OnDisable() => sharedVariable.onValueChanged.RemoveListener(OnVariableChanged);

        void OnVariableChanged(T variable) => onValueChanged?.Invoke(variable);
    }

    public class SharedComponentAssigner<T, TShared> : SharedVariableAssigner<T, TShared>
        where T : UnityEngine.Component
        where TShared : SharedVariable<T>
    {
        public bool checkParents;
        public bool checkChildren;
        protected override T GetVariableAssignment()
        {
            if (checkChildren)
            {
                return GetComponentInChildren<T>();
            }
            return GetComponent<T>();
        }
    }

    public class SharedComponentsAssigner<T, TShared> : SharedArrayAssigner<T, TShared>
        where T :  Component
        where TShared : SharedArray<T>
    {
        protected override T[] GetVariableAssignment() => GetComponentsInChildren<T>();
    }

    public abstract class SharedArrayAssigner<T, TShared> : MonoBehaviour
        where T : class
        where TShared : SharedArray<T>
    {
        [SerializeField] private TShared sharedArray;
        protected abstract T[] GetVariableAssignment();

        private void OnEnable() => sharedArray.Value = GetVariableAssignment();
    }
   
    public abstract class SharedVariableAssigner<T, TShared> : MonoBehaviour
        where T : UnityEngine.Object
        where TShared : SharedVariable<T>
    {
        [SerializeField] private TShared sharedVariable;
        
        protected abstract T GetVariableAssignment();

        private void OnEnable() => sharedVariable.Value = GetVariableAssignment();
    }
    
    
#if UNITY_EDITOR
    
    public class SharedVariableEditor<T, TShared> : Editor 
        where T : UnityEngine.Object where TShared : SharedVariable<T>
    {
        public override void OnInspectorGUI()
        {
            OnVariableGUI(target as TShared);
            base.OnInspectorGUI();
        }

        protected virtual void OnVariableGUI( TShared t)
        {
            if (t.HasValue)
            {
                string label = $"<b>{t.Value.name}</b>";
                GUILayout.Box(new GUIContent(label));
                EditorGUILayout.ObjectField("Shared Reference:", t.Value, t.Value.GetType());
            }
        }
    }
    
    // public class SharedComponentEditor<T, TShared> : Editor 
    //     where T : UnityEngine.Component where TShared : SharedComponent<T>
    // {
    //     public override void OnInspectorGUI()
    //     {
    //         OnVariableGUI(target as TShared);
    //         base.OnInspectorGUI();
    //     }
    //
    //     protected virtual void OnVariableGUI( TShared t)
    //     {
    //         if (t.HasValue)
    //         {
    //             string label = $"<b>{t.Value.name}</b>";
    //             GUILayout.Box(new GUIContent(label));
    //             EditorGUILayout.ObjectField("Shared Reference:", t.Value, t.Value.GetType());
    //             
    //             EditorGUILayout.ObjectField("Component Reference", t.)
    //         }
    //     }
    // }
#endif

    // public class SharedGameObject : SharedVariable<GameObject>
    // {
    //     
    // }
    //
    // public class SharedComponent<T> : SharedGameObject where T: Component
    // {
    //     [SerializeField] private T component;
    //     
    //     public override GameObject Value
    //     {
    //         get => base.Value;
    //         set
    //         {
    //             base.Value = value;
    //             if (base.Value != null)
    //             {
    //                 component = value.GetComponent<T>();
    //                 if (component == null)
    //                 {
    //                     throw new MissingComponentException($"");
    //                 }
    //             }
    //         }
    //     }
    //
    //     public T ComponentValue
    //     {
    //         get
    //         {
    //             return component;
    //         }
    //     }
    // }
}