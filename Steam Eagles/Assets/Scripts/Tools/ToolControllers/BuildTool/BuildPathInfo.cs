using Buildings;
using UnityEngine;

namespace Tools.BuildTool
{
    public struct BuildPathInfo
    {
        public Vector3Int start;
        public Vector3Int end;
        public Building building;
        public BuildingLayers layer;
    }
}