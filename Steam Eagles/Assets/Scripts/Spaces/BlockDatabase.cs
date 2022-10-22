using System;
using Spaces;
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;


#endif
[CreateAssetMenu(menuName = "Steam Eagles/Block Database")]
public class BlockDatabase : ScriptableObject
{
    public List<BlockData> blocks;
}
