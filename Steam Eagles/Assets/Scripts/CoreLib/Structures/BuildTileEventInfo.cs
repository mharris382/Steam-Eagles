﻿using UnityEngine;

namespace CoreLib.Structures
{
    public struct TileEventInfo
    {
        public GameObject building;
        public Vector2Int tilePosition;
        public int layer;
        public Object tile;
        public Object oldTile;
        public CraftingEventInfoType type;
    }

    public struct PrefabEventInfo
    {
        public GameObject building;
        public Vector2Int tilePosition;
        public int layer;
        public bool isFlipped;
        public GameObject prefab;
        public CraftingEventInfoType type;
    }

public enum CraftingEventInfoType
    {
        DECONSTRUCT,
        BUILD,
        DAMAGED,
        SWAP,
        REPAIR
    }
}