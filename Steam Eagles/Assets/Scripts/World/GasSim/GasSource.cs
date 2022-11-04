﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Rand = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GasSim
{
    [AddComponentMenu("SteamEagles/Gas/GasSource")]
    public class GasSource : CellHelper, IGasSource
    {
        public Vector2Int size = new Vector2Int(4, 4);

        [Tooltip("Slower numbers are faster")] [Range(16, 1)] [SerializeField]
        private int slowdown = 1;


        [SerializeField] private bool useConstantAmount;
        [Range(0, 16)] [SerializeField] private int constantAmount = 1;

        [Range(0, 16)] public int amountMin = 1;
        [Range(1, 16)] public int amountMax = 1;

        [HideInInspector] public UnityEvent<int> onGasEvent;

        private int _count;

        public IEnumerable<(Vector2Int coord, int amount)> GetSourceCells()
        {
            _count++;
            if ((_count % slowdown) != 0) yield break;

            Vector2Int c0 = (Vector2Int)CellCoordinate;
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    int amt = useConstantAmount ? constantAmount : Rand.Range(amountMin, amountMax);
                    if (amt > 0)
                    {
                        Vector2Int offset = new Vector2Int(x, y);
                        yield return (c0 + offset, amt);
                    }
                }
            }
        }

        public void GasTakenFromSource(int amountTaken)
        {
            onGasEvent?.Invoke(amountTaken);
        }
    }


    // #if UNITY_EDITOR
    // public abstract class GasIOEditor : Editor
    // {
    //     private SerializedProperty onGasEvent;
    //     private SerializedProperty size;
    //     private SerializedProperty slowdown;
    //     private SerializedProperty useConstantAmount;
    //     private SerializedProperty constantAmount;
    //     private SerializedProperty amountMax;
    //     private SerializedProperty amountMin;
    //
    //     private void OnEnable()
    //     {
    //         this.onGasEvent = serializedObject.FindProperty("onGasEvent");
    //         this.size = serializedObject.FindProperty("slowdown");
    //         this.slowdown = serializedObject.FindProperty("slowdown");
    //         this.useConstantAmount = serializedObject.FindProperty("useConstantAmount");
    //         this.constantAmount = serializedObject.FindProperty("constantAmount");
    //         this.amountMin = serializedObject.FindProperty("amountMin");
    //         this.amountMax = serializedObject.FindProperty("amountMax");
    //         
    //     }
    //
    //     public override void OnInspectorGUI()
    //     {
    //
    //         EditorGUILayout.PropertyField(size);
    //         EditorGUILayout.PropertyField(slowdown);
    //         EditorGUILayout.PropertyField(useConstantAmount);
    //         if (useConstantAmount.boolValue)
    //         {
    //             EditorGUILayout.PropertyField(constantAmount);
    //         }
    //         else
    //         {
    //             EditorGUILayout.PropertyField(amountMin);
    //             EditorGUILayout.PropertyField(amountMax);
    //         }
    //         
    //         EditorGUILayout.PropertyField(onGasEvent, new GUIContent(GetEventLabel()));
    //     }
    //
    //     protected abstract string GetEventLabel();
    // }
    // [CanEditMultipleObjects()]
    // [CustomEditor(typeof(GasSource))]
    // public class GasSourceEditor : GasIOEditor
    // {
    //     protected override string GetEventLabel()
    //     {
    //         return "On Gas taken from source";
    //     }
    // }
    // [CanEditMultipleObjects()]
    // [CustomEditor(typeof(GasSink))]
    // public class GasSinkEditor : GasIOEditor
    // {
    //     protected override string GetEventLabel()
    //     {
    //         return "On Gas given to Sink";
    //     }
    // }
    // #endif
}