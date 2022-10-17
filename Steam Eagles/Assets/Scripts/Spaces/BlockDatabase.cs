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
