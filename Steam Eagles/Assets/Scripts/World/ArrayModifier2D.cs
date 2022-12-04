using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif


[RequireComponent(typeof(Rigidbody2D))]
public class JointConnection2D : MonoBehaviour
{
    private Joint2D _joint;
    public Joint2D Joint => _joint ? _joint : _joint = GetComponent<Joint2D>();

    
    private ReadOnlyReactiveProperty<Rigidbody2D> connectedBody;
    
    private void Awake()
    {
        connectedBody = new ReadOnlyReactiveProperty<Rigidbody2D>(_joint.ObserveEveryValueChanged(t => t.attachedRigidbody));
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(JointConnection2D))]
public class JointConnection2DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        string label = "";
        GUILayout.Box(new GUIContent(label));
        base.OnInspectorGUI();
    }
}
#endif

public abstract class JointVisual2D : MonoBehaviour
{
    private Joint2D _joint;
    

    public Joint2D Joint => _joint ? _joint : _joint = GetComponent<Joint2D>();
    
    private void Awake()
    {
        _joint = GetComponent<Joint2D>();
    }

    
}

public abstract class JointVisual2D<T> : JointVisual2D where T : Joint2D
{
    private T _joint;
    public new T Joint => _joint ? _joint : _joint = GetComponent<T>();
    

    private void Awake()
    {
        _joint = GetComponent<T>();
    }
    
    
    
}

[ExecuteAlways]
public class ArrayModifier2D : MonoBehaviour
{
    [Min(1)]
    [SerializeField] private int copyCount = 2;
    [SerializeField] private LocalOffset localOffset;
    [SerializeField] private AbsoluteOffset absoluteOffset;
    [SerializeField] private LerpOffset lerpOffset;
    [SerializeField] private TransformLerpOffset transformLerpOffset;
    [SerializeField] private TransformOffset transformOffset;
    [SerializeField] private SortingOrderOffset sortingOrderOffset;
    [SerializeField] private ColorOffset colorOffset;
    [SerializeField] internal List<GameObject> copies;
    
    
    private Renderer _renderer;
    internal Renderer Renderer => _renderer ? _renderer : _renderer = GetComponent<Renderer>();
    
    
    
    [System.Serializable]
    public class LocalOffset
    {
        public bool useLocalOffset = true;
        public Vector2 offset = Vector3.right;
        public float zOffset = 1;
        public void ApplyOffset(ArrayModifier2D arrayModifier2D, int index)
        {
            if (!useLocalOffset) return;
            var t = arrayModifier2D.transform.GetChild(index);
            var renderBounds = arrayModifier2D.GetComponentInChildren<UnityEngine.Renderer>().localBounds;
            var startPosition = arrayModifier2D.transform.position;
            var localOffset = new Vector3(renderBounds.size.x * (offset.x * index), renderBounds.size.x * offset.x * index, 0);
            var offset2D = startPosition + localOffset;
            var offsetZ = startPosition.z + (zOffset * index);
            t.position = new Vector3(offset2D.x, offset2D.y, offsetZ);
        }
    }
    
    [System.Serializable]
    public class LerpOffset
    {
        public bool useLerpOffset;
        public Vector3 offset = Vector3.right;
        
        public void ApplyOffset(ArrayModifier2D arrayModifier2D, int i)
        {
            if (!useLerpOffset) return;
            var cnt = arrayModifier2D.transform.childCount;
            var t = i / (float) cnt;
            var child = arrayModifier2D.transform.GetChild(i);
            var p0 = arrayModifier2D.transform.position;
            var p1 = arrayModifier2D.transform.TransformPoint(offset);
            child.position = Vector3.Lerp(p0, p1, t);
        }
    }
    [System.Serializable]
    public class TransformLerpOffset
    {
        public bool useLerpOffset;


       [SerializeField] private Transform target;
       [SerializeField] private Vector3 targetOffset;
       [SerializeField] private Transform from; 
       [SerializeField] private Vector3 fromOffset;
        public void ApplyOffset(ArrayModifier2D arrayModifier2D, int i)
        {
            if (!useLerpOffset) return;
            if (target == null)
                return;

            var cnt = arrayModifier2D.transform.childCount;
            var t = i / (float) cnt;
            var child = arrayModifier2D.transform.GetChild(i);
            
            
            var transformFrom = @from ? @from : arrayModifier2D.transform;
            var p0 = target.TransformPoint(targetOffset);
            var p1 = transformFrom.TransformPoint(fromOffset);
            
            if (child == target) return;
            if (child == transformFrom) return;
           
            child.position = Vector3.Lerp(p0, p1, t);
        }
    }
    
    [System.Serializable]
    public class AbsoluteOffset
    {
        public bool useAbsoluteOffset = false;
        public Vector3 offset = Vector3.right;

        public void ApplyOffset(ArrayModifier2D arrayModifier2D, int i)
        {
            if (!useAbsoluteOffset) return;
            var t = arrayModifier2D.transform.GetChild(i);
            var startPosition = arrayModifier2D.transform.position;
            var localOffset = new Vector3(offset.x * i, offset.y * i, offset.z*i);
            if (arrayModifier2D.localOffset.useLocalOffset)
            {
               t.position += localOffset;
            }
            else
            {
               t.position = startPosition + localOffset;
            }
            
        }
    }
    
    [System.Serializable]
    public class TransformOffset
    {
        public bool useTransformOffset = false;
        public Transform offsetObject;
        

        public void ApplyOffset(ArrayModifier2D arrayModifier2D, int i)
        {
            if(!useTransformOffset || !offsetObject) return;
            
            var t = arrayModifier2D.transform.GetChild(i);
            var startPosition = arrayModifier2D.transform.position;
            var startRotation = arrayModifier2D.transform.rotation;
            var startScale = arrayModifier2D.transform.localScale;
            
            var positionOffset = offsetObject.position - startPosition;
            var angleOffset = Vector2.SignedAngle(arrayModifier2D.transform.right, offsetObject.right);
            var rotationOffset = Quaternion.Euler(0, 0, i * angleOffset);
            
            var scaleOffset = offsetObject.localScale - arrayModifier2D.transform.localScale;
            var scale = scaleOffset * i;

            if (arrayModifier2D.localOffset.useLocalOffset || arrayModifier2D.absoluteOffset.useAbsoluteOffset)
            {
                t.position += positionOffset * i;
            }
            else
            {
                t.position = startPosition + positionOffset * i;
            }
            
            t.rotation = startRotation * rotationOffset;
            t.localScale = Vector3.one + scale;
        }
    }

    [System.Serializable]
    public class ColorOffset
    {
        public bool useColorOffset;
        public Gradient gradient;

        public void ApplyOffset(ArrayModifier2D arrayModifier2D, int i)
        {
            if (!useColorOffset) return;
            float t = i / (float)arrayModifier2D.transform.childCount;
            var color = gradient.Evaluate(t);
            var tr = arrayModifier2D.transform.GetChild(i);
            var sr = tr.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = color;
                return;
            }
            if (Application.isPlaying == false) return;
            var renderer = arrayModifier2D.transform.GetChild(i).GetComponent<Renderer>();
           
            renderer.material.color = color;
        }
    }

    [Serializable]
    public class SortingOrderOffset
    {
        public bool useSortingOrderOffset;
        public int startOrder = 0;
        public int offset = -1;

        private int _currentOrder;
        public void Apply(ArrayModifier2D arrayModifier2D, int i)
        {
            if (!useSortingOrderOffset) return;
            if (i == 0)
            {
                _currentOrder = startOrder;
            }
            else
            {
                _currentOrder += offset;
            }
            var renderer = arrayModifier2D.transform.GetChild(i).GetComponent<Renderer>();
            switch (renderer.GetType())
            {
                case var _ when renderer is SpriteRenderer:
                    var spriteRenderer = renderer as SpriteRenderer;
                    spriteRenderer.sortingOrder = _currentOrder;
                    break;
                case var _ when renderer is ParticleSystemRenderer:
                    var particleSystemRenderer = renderer as ParticleSystemRenderer;
                    particleSystemRenderer.sortingOrder = _currentOrder;
                    break;
            }
        }
    }
    [SerializeField] 
    private Transform _copyParent;

    internal Transform copyParent
    {
        get
        {
            if (_copyParent == null)
            {
                _copyParent = new GameObject("ArrayModifier2D").transform;
                _copyParent.hideFlags = HideFlags.HideInHierarchy;
                _copyParent.SetParent(transform.parent, false);
            }
            return _copyParent;
        }
    }


    private int targetCount => transform.childCount;
    
    
    
    GameObject CreateCopy()
    {
        var copy = new GameObject("Copy");
        copy.transform.SetParent(copyParent, false);
        copy.transform.localPosition = transform.localPosition;
        copy.transform.localRotation = transform.localRotation;
        copy.transform.localScale = transform.localScale;
        copy.hideFlags = HideFlags.HideInHierarchy;
        return copy;
    }

    private void Update()
    {
        PositionCopies();
    }

    internal void PositionCopies()
    {

        if (Application.isPlaying)
        {
            enabled = false;
            return;
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            localOffset.ApplyOffset(this, i);
            lerpOffset.ApplyOffset(this, i);
            transformLerpOffset.ApplyOffset(this, i);
            absoluteOffset.ApplyOffset(this, i);
            transformOffset.ApplyOffset(this, i);
            colorOffset.ApplyOffset(this, i);
            sortingOrderOffset.Apply(this, i);
        }
    }

    internal void CreateCopies()
    {
    return;
        if (copies == null)
        {
            copies = new List<GameObject>(this.targetCount);
        }
        int currentCount = copies.Count;
        int targetCount = this.targetCount;
        if (currentCount == targetCount)
        {
            return;
        }

        if (currentCount > targetCount)
        {
            //destroy copies
            for (int i = currentCount; i > targetCount; i--)
            {
                var copyToDestroy = copies[i - 1];
                copies.RemoveAt(i - 1);
                if (Application.isPlaying)
                {
                    Destroy(copyToDestroy);
                }
                else
                {
                    DestroyImmediate(copyToDestroy);
                }
            }
        }
        else
        {
            //create copies
            for (int i = currentCount; i < targetCount; i++)
            {
                var copy = CreateCopy();
                copies.Add(copy);
            }
        }
    }
}



// #if UNITY_EDITOR
// [CustomEditor(typeof(ArrayModifier2D))]
// public class ArrayModifier2DEditor : Editor
// {
//     private SerializedProperty _copyCount;
//     private SerializedProperty _copies;
//     private SerializedProperty _localOffset;
//     private SerializedProperty _transformOffset;
//     private SerializedProperty _absoluteOffset;
//     private SerializedProperty _localOffset_useOffset;
//     private SerializedProperty _localOffset_offset;
//     private SerializedProperty _absoluteOffset_useOffset;
//     private SerializedProperty _absoluteOffset_offset;
//     private SerializedProperty _transformOffset_useOffset;
//     private SerializedProperty _transformOffset_offset;
//
//     private void OnEnable()
//     {
//         this._copyCount = serializedObject.FindProperty("copyCount");
//         this._copies = serializedObject.FindProperty("copies");
//         this._localOffset = serializedObject.FindProperty("localOffset");
//         this._localOffset_useOffset = _localOffset.FindPropertyRelative("useLocalOffset");
//         this._localOffset_offset = _localOffset.FindPropertyRelative("offset");
//         this._absoluteOffset = serializedObject.FindProperty("absoluteOffset");
//         this._absoluteOffset_useOffset = _absoluteOffset.FindPropertyRelative("useAbsoluteOffset");
//         this._absoluteOffset_offset = _absoluteOffset.FindPropertyRelative("offset");
//         this._transformOffset = serializedObject.FindProperty("transformOffset");
//         this._transformOffset_useOffset = _transformOffset.FindPropertyRelative("useTransformOffset");
//         this._transformOffset_offset = _transformOffset.FindPropertyRelative("offsetObject");
//         
//     }
//     
//     void DrawOffsetField(SerializedProperty property, SerializedProperty usePropertyProperty, SerializedProperty offsetProperty)
//     {
//         
//         EditorGUILayout.BeginHorizontal();
//         if (EditorGUILayout.Toggle(usePropertyProperty.boolValue, EditorStyles.toggle, GUILayout.Width(100)))
//         {
//             EditorGUI.indentLevel++;
//             EditorGUILayout.PropertyField(offsetProperty);
//             EditorGUI.indentLevel--;
//         }
//         EditorGUILayout.EndHorizontal();
//     }
//     
//     
//
//     public override void OnInspectorGUI()
//     {
//         var am = target as ArrayModifier2D;
//         serializedObject.Update();
//         GUIContent copyCountContent = new GUIContent("Count");
//         EditorGUI.BeginChangeCheck();
//         EditorGUILayout.PropertyField(this._copyCount);
//         if (EditorGUI.EndChangeCheck())
//         {
//             
//             am.CreateCopies();
//         }
//         EditorGUI.BeginChangeCheck();
//         DrawOffsetField(_localOffset, _localOffset_useOffset, _localOffset_offset);
//         DrawOffsetField(_absoluteOffset, _absoluteOffset_useOffset, _absoluteOffset_offset);
//         DrawOffsetField(_transformOffset, _transformOffset_useOffset, _transformOffset_offset);
//         if (EditorGUI.EndChangeCheck())
//         {
//             am.PositionCopies();
//         }
//
//         EditorGUILayout.PropertyField(_copies);
//         serializedObject.ApplyModifiedProperties();
//     }
// }
// #endif