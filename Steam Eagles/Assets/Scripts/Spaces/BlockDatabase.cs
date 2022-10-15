using System;
using Spaces;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;


#endif
[CreateAssetMenu(menuName = "Steam Eagles/Block Database")]
public class BlockDatabase : ScriptableObject
{
    public List<BlockMapType> mapTypes;
    public List<BlockData> blocks;
}

#if UNITY_EDITOR

[CustomEditor(typeof(BlockDatabase))]
public class BlockLibraryEditor : Editor
{
    private SerializedProperty _pMapTypes;
    private SerializedProperty _pBlocks;
    private void OnEnable()
    {
        _pMapTypes = serializedObject.FindProperty("mapTypes");
        _pBlocks = serializedObject.FindProperty("blocks");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        List<BlockData> invalidBlocksFound  = new List<BlockData>();
        var bl = target as BlockDatabase;
        bl.blocks.RemoveAll(t => t == null);
        GUILayout.BeginVertical();
        {
            if (GUILayout.Button("Create New Block"))
            {
                BlockData blockData = ScriptableObject.CreateInstance<BlockData>();
                AssetDatabase.CreateAsset(blockData, "Assets/Data/New Block");
                AssetDatabase.SaveAssets();
                _pBlocks.InsertArrayElementAtIndex(_pBlocks.arraySize);
            }

            if (GUILayout.Button("Create New Map Type"))
            {
                BlockMapType mapType = ScriptableObject.CreateInstance<BlockMapType>();
                AssetDatabase.CreateAsset(mapType, "Assets/Data/New Map Type");
                AssetDatabase.SaveAssets();
                _pMapTypes.InsertArrayElementAtIndex(_pBlocks.arraySize);
            }
        }

        List<(int, string, Color)> warnings = new List<(int, string, Color)>();
        
        foreach (var blockData in bl.blocks)
        {
             InvalidBlockReason reason = InvalidBlockReason.NULL;
             if (!IsBlockValid(blockData, ref reason))
             {
                 AddWarning(blockData, reason, warnings);
             }
        }
        warnings.Sort((t, v)=> t.Item1 - v.Item1);
        
        foreach (var valueTuple in warnings)
        {
            GUI.backgroundColor = valueTuple.Item3/2;
            GUI.contentColor = valueTuple.Item3;
            GUILayout.Box(new GUIContent(valueTuple.Item2));    
        }
        
        base.OnInspectorGUI();
        serializedObject.ApplyModifiedProperties();
    }

    private void AddWarning(BlockData blockData, InvalidBlockReason reason, List<(int, string, Color)> warnings)
    {
        throw new NotImplementedException();
    }


    bool IsBlockValid(BlockData blockData, ref InvalidBlockReason reason)
    {
        if (blockData == null)
        {
            reason = InvalidBlockReason.NULL;
            return false;
        }

        if (blockData.blockType == null)
        {
            reason = InvalidBlockReason.MISSING_BLOCKTYPE;
            return false;
        }
        if (blockData.dynamicBlock== null)
        {
            reason = InvalidBlockReason.MISSING_DYNAMIC;
            return false;
            
        }
        if (blockData.staticBlock == null)
        {
            reason = InvalidBlockReason.MISSING_STATIC;
            return false;
        }
        if (blockData.staticBlock.sprite == null)
        {
            reason = InvalidBlockReason.MISSING_STATIC_SPRITE;
            return false;
        }
        return true;
    }

    enum InvalidBlockReason
    {
        MISSING_STATIC,
        MISSING_DYNAMIC,
        MISSING_STATIC_SPRITE,
        NULL,
        MISSING_BLOCKTYPE
    }
}
#endif