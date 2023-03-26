using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PhysicsFun.Buildings.SaveLoad
{
    [CreateAssetMenu(fileName = "TileDatabase", menuName = "Steam Eagles/Databases/TileDatabase", order = 0)]
    public class TileDatabase : ScriptableObject
    {
        private static TileDatabase _instance;
        public static TileDatabase Instance => (_instance != null) ? _instance : _instance = Resources.Load<TileDatabase>("TileDatabase");
        

        /// <summary>
        /// set of 1 or more tiles that can be used to build a building.
        /// One core tile is assigned as the "main" tile, and the rest are "sub" tiles.
        /// main tiles are used to load the main building from a save file.
        /// <para>
        ///The name of the main tile is used to identify the tileset
        /// </para>
        /// </summary>
        [Serializable]        
        public class NamedTileSet
        {
            [AssetList(AutoPopulate = true, Path = "Assets/Tiles"), Required]
            public TileBase mainTile;
            
            [AssetList(AutoPopulate = true, Path = "Assets/Tiles")]
            [ShowIf(nameof(HasMainTile))]
            public TileBase[] subTiles;

            private bool HasMainTile => mainTile != null;
            
            public string Name => HasMainTile ?  mainTile.name : "";
        }
        
        
        [SerializeField, TableList(ShowIndexLabels = true)]
        private NamedTileSet[] tileSets;


        private Dictionary<TileBase, string> tileToName = new Dictionary<TileBase, string>();

        
        public TileBase GetTileByName(string name)
        {
            foreach (var tileSet in tileSets)
            {
                if (tileSet.Name == name)
                {
                    return tileSet.mainTile;
                }
            }
            return null;
        }
        
        public string GetTileName(TileBase tile)
        {
            if (tileToName.TryGetValue(tile, out var n))
            {
                return n;
            }

            foreach (var namedTileSet in tileSets)
            {
                foreach (var subtile in namedTileSet.subTiles)
                {
                    if (subtile == tile)
                    {
                        tileToName.Add(tile, namedTileSet.Name);
                        return namedTileSet.Name;
                    }
                }
            }
            tileToName.Add(tile, "");
            return "";
        }
    }
}