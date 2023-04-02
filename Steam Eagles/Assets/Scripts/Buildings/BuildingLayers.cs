using System;
using System.Collections.Generic;
using System.Linq;
using Buildings.BuildingTilemaps;
using UnityEngine;

namespace Buildings
{
    [Flags]
    public enum BuildingLayers
    {
        NONE,
        WALL=1,
        FOUNDATION=2,
        SOLID=4,
        PIPE=8,
        COVER=16,
        PLATFORM=32,
        DECOR=64,
        WIRES=128,
        LADDERS=256,
        REQUIRED = FOUNDATION | SOLID | PIPE | WIRES,
    }


    public static class BuildingLayersExtensions
    {
        private static Dictionary<BuildingLayers, string> _tag;
        private static Dictionary<BuildingLayers, string> _sortingLayerName;
        private static Dictionary<BuildingLayers, int> _layer;
        private static Dictionary<BuildingLayers, int> _orderInLayer;
        private static Dictionary<BuildingLayers, bool> _hasCollider;
        private static Dictionary<BuildingLayers, bool> _isTrigger;
        private static Dictionary<BuildingLayers, bool> _isPlatform;


        static BuildingLayersExtensions()
        {
            Dictionary<BuildingLayers, string> _tag = new Dictionary<BuildingLayers, string>();
            Dictionary<BuildingLayers, string> _sortingLayerName = new Dictionary<BuildingLayers, string>();
            Dictionary<BuildingLayers, int> _layer = new Dictionary<BuildingLayers, int>();
            Dictionary<BuildingLayers, int> _orderInLayer = new Dictionary<BuildingLayers, int>();
            Dictionary<BuildingLayers, bool> _hasCollider = new Dictionary<BuildingLayers, bool>();
            Dictionary<BuildingLayers, bool> _isTrigger = new Dictionary<BuildingLayers, bool>();
            Dictionary<BuildingLayers, bool> _isPlatform = new Dictionary<BuildingLayers, bool>();

            var enums = Enum.GetValues(typeof(BuildingLayers)).Cast<BuildingLayers>()
                .Where(t => t != BuildingLayers.NONE || t != BuildingLayers.REQUIRED).ToList();
            AddTags();
            AddSortingLayers();
            AddLayers();
            SpecifyColliders(BuildingLayers.SOLID, BuildingLayers.FOUNDATION, BuildingLayers.PIPE, BuildingLayers.LADDERS, BuildingLayers.PLATFORM);
            SpecifyPlatforms(BuildingLayers.PIPE, BuildingLayers.PLATFORM);
            SpecifyTriggers(BuildingLayers.LADDERS);

            SpecifyDefaults(enums);
        }

        static void AddLayers()
        {
            _layer.Add(BuildingLayers.SOLID, LayerMask.NameToLayer("Solids"));
            _layer.Add(BuildingLayers.PIPE, LayerMask.NameToLayer("Pipes"));
            _layer.Add(BuildingLayers.PLATFORM, LayerMask.NameToLayer("Platforms"));
            _layer.Add(BuildingLayers.LADDERS, LayerMask.NameToLayer("Triggers"));
            _layer.Add(BuildingLayers.FOUNDATION, LayerMask.NameToLayer("Ground"));
            _layer.Add(BuildingLayers.WIRES, LayerMask.NameToLayer("Wires"));
            
        }

    static void SpecifyDefaults(List<BuildingLayers> layers)
        {
            foreach (var layer in layers)
            {
                if(!_tag.ContainsKey(layer))_tag.Add(layer, "");
                if(!_sortingLayerName.ContainsKey(layer))_sortingLayerName.Add(layer, "Default");
                if(!_layer.ContainsKey(layer))_layer.Add(layer, 0);
                if(!_orderInLayer.ContainsKey(layer))_orderInLayer.Add(layer, 0);
                if(!_hasCollider.ContainsKey(layer))_hasCollider.Add(layer, false);
                if(!_isTrigger.ContainsKey(layer))_isTrigger.Add(layer, false);
                if(!_isPlatform.ContainsKey(layer))_isPlatform.Add(layer, false);
            }
        }
        static void AddTags()
        {
            _tag.Add(BuildingLayers.PIPE, "Pipe Tilemap");
            _tag.Add(BuildingLayers.SOLID, "Solid Tilemap");
            _tag.Add(BuildingLayers.WALL, "Wall");
            _tag.Add(BuildingLayers.LADDERS, "Ladder");
        }
        static void AddSortingLayers()
        {
            _sortingLayerName.Add(BuildingLayers.PIPE, "Near BG");
            _sortingLayerName.Add(BuildingLayers.FOUNDATION, "Near BG");
            _sortingLayerName.Add(BuildingLayers.WALL, "Near BG");
            _sortingLayerName.Add(BuildingLayers.SOLID, "Near BG");
            _sortingLayerName.Add(BuildingLayers.WIRES, "Near BG");

            _orderInLayer.Add(BuildingLayers.FOUNDATION, -3);
            _orderInLayer.Add(BuildingLayers.SOLID, -5);
            _orderInLayer.Add(BuildingLayers.PIPE, -6);
            _orderInLayer.Add(BuildingLayers.WIRES, -10);
            _orderInLayer.Add(BuildingLayers.WALL, -25);
            
            _sortingLayerName.Add(BuildingLayers.COVER, "Near FG");
        }
        
        static void SpecifyColliders(params BuildingLayers[] layers)
        {
            foreach (var layer in layers)
            {
                if(_hasCollider.ContainsKey(layer)==false)
                    _hasCollider.Add(layer, true);
                else
                    _hasCollider[layer] = true;
            }   
        }
        
        static void SpecifyTriggers(params BuildingLayers[] layers)
        {
            foreach (var layer in layers)
            {
                if(_isTrigger.ContainsKey(layer)==false)
                    _isTrigger.Add(layer, true);
                else
                    _isTrigger[layer] = true;
            }   
        }
        
        static void SpecifyPlatforms(params BuildingLayers[] layers)
        {
            foreach (var layer in layers)
            {
                if(_isPlatform.ContainsKey(layer)==false)
                    _isPlatform.Add(layer, true);
                else
                    _isPlatform[layer] = true;
            }
        }
        
        public static Type GetBuildingTilemapType(this BuildingLayers layers)
        {
            switch (layers)
            {
                case BuildingLayers.NONE:
                    return null;
                    break;
                case BuildingLayers.WALL:
                    return typeof(WallTilemap);
                    break;
                case BuildingLayers.FOUNDATION:
                    return typeof(FoundationTilemap);
                    break;
                case BuildingLayers.SOLID:
                    return typeof(SolidTilemap);
                    break;
                case BuildingLayers.PIPE:
                    return typeof(PipeTilemap);
                    break;
                case BuildingLayers.COVER:
                    return typeof(CoverTilemap);
                    break;
                case BuildingLayers.PLATFORM:
                    return typeof(PlatformTilemap);
                    break;
                case BuildingLayers.DECOR:
                    return typeof(DecorTilemap);
                    break;
                case BuildingLayers.WIRES:
                    return typeof(WireTilemap);
                    break;
                case BuildingLayers.LADDERS:
                    return typeof(LadderTilemap);
                    break;
                case BuildingLayers.REQUIRED:
                    throw new InvalidOperationException("Cannot request a type for a layer that is not a single layer.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(layers), layers, null);
            }
        }

        public static TilemapBuilderParameters GetBuilderParameters(this BuildingLayers layers)
        {
            string tag = _tag.ContainsKey(layers) ? _tag[layers] : "";
            int layer = _layer.ContainsKey(layers) ? _layer[layers] : 0;
            bool isRendered = true;
            bool hasCollider = _hasCollider.ContainsKey(layers) && _hasCollider[layers];
            bool isTrigger = _isTrigger.ContainsKey(layers) && _isTrigger[layers];
            int sortingOrder = _orderInLayer.ContainsKey(layers) ? _orderInLayer[layers] : 0;
            string sortingLayer = _sortingLayerName.ContainsKey(layers) ? _sortingLayerName[layers] : "Default";

            return new TilemapBuilderParameters(tilemapType:layers.GetBuildingTilemapType(), isRendered, withCollider:hasCollider, asTrigger:isTrigger, 
                sortingLayerName:sortingLayer, sortingOrder:sortingOrder, tag:tag, layer:layer);
        }
        public static IEnumerable<Type> GetBuildingTilemapTypes(this BuildingLayers layers)
        {
            foreach (var value in Enum.GetValues(typeof(BuildingLayers)))
            {
                BuildingLayers l = value as BuildingLayers? ?? BuildingLayers.NONE;
                if(l == BuildingLayers.NONE || l == BuildingLayers.REQUIRED)
                    continue;
                if (layers.HasFlag(l))
                {
                    yield return l.GetBuildingTilemapType();
                }
            }
        }
    }
}