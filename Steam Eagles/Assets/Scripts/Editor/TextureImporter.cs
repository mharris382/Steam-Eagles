using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SteamEagles.CustomEditor
{
    public class CustomTextureImporter : AssetPostprocessor
    {
        const string IMPORT_PATH_ROOT = "Assets/Textures/";
        const string HEIGHT_MAP_FORMAT = "{0}_height";
        const string ROUGHNESS_MAP_FORMAT = "{0}_roughness";
        const string BASE_MAP_FORMAT = "{0}_basecolor";
        const string OPACITY_MAP_FORMAT = "{0}_opacity";

        private static string[] ColorKeywords = new[]
        {
            "COLOR",
            "DIFFUSE",
            "BASECOLOR"
        };
        private static string ROUGHNESS_KEYWORD = "ROUGHNESS";
        private static string METALLIC_KEYWORD = "METALLIC";
        private static string HEIGHT_KEYWORD = "HEIGHT";
        private static string OPACITY_KEYWORD = "OPACITY";

        //
        // private void OnPostprocessTexture(Texture2D texture)
        // {
        //     var path = assetPath;
        //     if (path.StartsWith(IMPORT_PATH_ROOT))
        //     {
        //         var parts = texture.name.Split('_');
        //         if(parts.Length < 2)
        //             return;
        //         
        //         StringBuilder sb = new StringBuilder();
        //         for (int i = 0; i < parts.Length-1; i++)
        //         {
        //             sb.Append(parts[i]);
        //             sb.Append(' ');
        //         }
        //         Debug.Log("Texture name: " + sb.ToString());
        //         var suffix = parts[parts.Length - 1];
        //         var mapType = ImporterHelper.GetMapTypeFromSuffix(suffix);
        //         if(mapType == MapType.Unknown)
        //         {
        //             Debug.LogWarning("Unknown map type: " + suffix);
        //             return;
        //         }
        //         Debug.Log($"{texture.name} is Map type: " + mapType);
        //         return;
        //         
        //         var textureImporter = assetImporter as TextureImporter;
        //         if (textureImporter != null)
        //         {
        //             var textureName = path.Substring(IMPORT_PATH_ROOT.Length);
        //             textureImporter.textureType = TextureImporterType.Default;
        //             textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        //             textureImporter.mipmapEnabled = false;
        //             textureImporter.wrapMode = TextureWrapMode.Clamp;
        //             textureImporter.filterMode = FilterMode.Point;
        //             textureImporter.anisoLevel = 0;
        //             textureImporter.maxTextureSize = 2048;
        //             textureImporter.isReadable = true;
        //             textureImporter.alphaIsTransparency = true;
        //             textureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
        //             textureImporter.spriteImportMode = SpriteImportMode.None;
        //             textureImporter.textureType = TextureImporterType.Default;
        //             textureImporter.npotScale = TextureImporterNPOTScale.None;
        //             textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
        //             textureImporter.SaveAndReimport();
        //         }
        //     }
        // }

       
    }


    [Flags]
    public  enum MapType
    {
        Height,
        Roughness,
        BaseColor,
        Opacity,
        Metallic,
        Unknown
    }

    public static class ImporterHelper
    {
        
        private static Dictionary<string, MapType> _keywordDictionary;
        
        static ImporterHelper()
        {
            _keywordDictionary = new Dictionary<string, MapType>();
            _keywordDictionary.Add("METALLIC", MapType.Metallic);
            _keywordDictionary.Add("ROUGHNESS", MapType.Roughness);
            _keywordDictionary.Add("HEIGHT", MapType.Height);
            
            _keywordDictionary.Add("BASECOLOR", MapType.BaseColor);
            _keywordDictionary.Add("COLOR", MapType.BaseColor);
            
            _keywordDictionary.Add("OPACITY", MapType.Opacity);
        }


        public static MapType GetMapTypeFromSuffix(string suffix)
        {
            suffix = suffix.ToUpper();
            if (_keywordDictionary.ContainsKey(suffix))
                return _keywordDictionary[suffix];
            return MapType.Unknown;
        }
    }
}