using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[RequireComponent(typeof(SpriteRenderer))]
public class PoweredLight : MonoBehaviour
{
    [SerializeField] private Material materialTemplate;
    
    private SpriteRenderer _sr;
    [SerializeField] private Material material;
    
    [SerializeField, Range(0,1 )]
    private float power;

    private static readonly int Power1 = Shader.PropertyToID("_Power");

    public float Power
    {
        get => power;
        set
        {
            value = Mathf.Clamp01(value);
            if (Math.Abs(value - power) > Mathf.Epsilon)
            {
                power = value;
                PowerMat.SetFloat(Power1, power);
            }
        }
    }

    public Material PowerMat
    {
        get
        {
            if (material == null)
            {
                if (materialTemplate == null)
                {
                    materialTemplate = SpriteRenderer.material;
                }
                material = new Material(materialTemplate);
            }        
            return material;
        }
    }

    public SpriteRenderer SpriteRenderer => _sr == null ? (_sr = GetComponent<SpriteRenderer>()) : _sr;
    
    
    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        material = new Material(materialTemplate);
    }

    
}


#if UNITY_EDITOR
[CustomEditor(typeof(PoweredLight))]
public class PoweredLightEditor : Editor
{
    private SerializedProperty _power;

    private void OnEnable()
    {
        this._power = serializedObject.FindProperty("power");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUI.BeginChangeCheck();
        _power.floatValue = EditorGUILayout.Slider(_power.floatValue, 0, 1);
        if (EditorGUI.EndChangeCheck())
        {
            var poweredLight = (PoweredLight) target;
            poweredLight.SpriteRenderer.material = poweredLight.PowerMat;
            poweredLight.Power = _power.floatValue;
        }
        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }
}
#endif