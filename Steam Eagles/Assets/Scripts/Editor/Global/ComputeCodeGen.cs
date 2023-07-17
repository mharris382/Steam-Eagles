using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using System.IO;
using UnityEngine;

namespace MyEditor.Global
{
    public class ComputeCodeGen : OdinEditorWindow
    {
        [OnValueChanged(nameof(GetStringFromAsset))]
        public ComputeShader computeShader;
        
        [Sirenix.OdinInspector.FilePath(Extensions = ".compute", ParentFolder = "Resources/Computes")]
        public string pathToComputeShader;

        [Sirenix.OdinInspector.FilePath(Extensions = ".cs")]
        public string pathToCsharpShaderCounterpart;

        public void GetStringFromAsset(ComputeShader cs)
        {
            if (cs == null) return;
            var path = AssetDatabase.GetAssetPath(cs);
            var text = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            pathToComputeShader = path;
        }

        [MenuItem("Tools/Compute Code Gen")]
        private static void ShowWindow()
        {
            var window = GetWindow<ComputeCodeGen>();
            window.titleContent = new GUIContent("Compute Code Gen");
            window.Show();
        }

        protected override IEnumerable<object> GetTargets()
        {
            yield return this;
        }


        bool CheckHasFiles()
        {
            if (string.IsNullOrEmpty(pathToComputeShader) || string.IsNullOrEmpty(pathToCsharpShaderCounterpart))
                return false;
            
            if (computeShader == null)
                return false;
            
            return true;
        }



        void SearchComputeShaderLines()
        {
            
        }

        public struct ComputeShaderStructure
        {
            
        }
        
        public struct ComputeShaderParameter
        {
            public string name;
            public ComputeShaderParameterTypes type;
            
        }
        
        public struct ComputeKernelName
        {
            public string kernelName;
            public int xCount;
            public int yCount;
            public int zCount;
        }
        
        public enum ComputeShaderParameterTypes
        {
            TEXTURE2D, BUFFER, INT, INT2, INT3, INT4, Float, Float2, Float3, Float4
        }

        static class ComputeShaderHelper
        {
            private static Dictionary<string, ComputeShaderParameterTypes> _keywordToEnum = new();

            static ComputeShaderHelper()
            {
                _keywordToEnum.Add("RWTexture2D<float4>", ComputeShaderParameterTypes.TEXTURE2D);
                _keywordToEnum.Add("RWTexture2D<float3>", ComputeShaderParameterTypes.TEXTURE2D);
                _keywordToEnum.Add("RWTexture2D<float2>", ComputeShaderParameterTypes.TEXTURE2D);
                _keywordToEnum.Add("RWTexture2D<float>", ComputeShaderParameterTypes.TEXTURE2D);
                _keywordToEnum.Add("Texture2D<float4>", ComputeShaderParameterTypes.TEXTURE2D);
                _keywordToEnum.Add("Texture2D<float3>", ComputeShaderParameterTypes.TEXTURE2D);
                _keywordToEnum.Add("Texture2D<float2>", ComputeShaderParameterTypes.TEXTURE2D);
                _keywordToEnum.Add("Texture2D<float>", ComputeShaderParameterTypes.TEXTURE2D);
                
                _keywordToEnum.Add("int", ComputeShaderParameterTypes.INT);
                _keywordToEnum.Add("int2", ComputeShaderParameterTypes.INT2);
                _keywordToEnum.Add("int3", ComputeShaderParameterTypes.INT3);
                _keywordToEnum.Add("int4", ComputeShaderParameterTypes.INT4);
                _keywordToEnum.Add("uint", ComputeShaderParameterTypes.INT);
                _keywordToEnum.Add("uint2", ComputeShaderParameterTypes.INT2);
                _keywordToEnum.Add("uint3", ComputeShaderParameterTypes.INT3);
                _keywordToEnum.Add("uint4", ComputeShaderParameterTypes.INT4);
                
                _keywordToEnum.Add("float", ComputeShaderParameterTypes.Float);
                _keywordToEnum.Add("float2", ComputeShaderParameterTypes.Float2);
                _keywordToEnum.Add("float3", ComputeShaderParameterTypes.Float3);
                _keywordToEnum.Add("float4", ComputeShaderParameterTypes.Float4);
            }
        }
    }
}