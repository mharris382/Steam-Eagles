using System;
using _EXP.PhysicsFun.ComputeFluid.Utilities;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public class SamplePoint : MonoBehaviour
    {
        private SamplePoints _samplePoints;

        public SpriteRenderer spriteRenderer;
        public Gradient gradient;
        private GasTexture _gasTexture;
        private TextureMap _textureMap;


        [ShowInInspector, BoxGroup("Debugging"),HideInEditorMode]
        public bool InBounds
        {
            get;
            set;
        }
        [ShowInInspector, BoxGroup("Debugging"),HideInEditorMode]
        public Vector2Int Texel
        {
            get;
            set;
        }
        [ShowInInspector, BoxGroup("Debugging/Output"),HideInEditorMode]
        public float GasValue
        {
            get;
            set;
        }
        
        [ShowInInspector, BoxGroup("Debugging/Output"),HideInEditorMode]
        public float MaxValue
        {
            get;
            set;
        }
        [ShowInInspector, BoxGroup("Debugging/Output"),HideInEditorMode]
        public float Total
        {
            get;
            set;
        }
        [ShowInInspector, BoxGroup("Debugging/Output"),HideInEditorMode]
        public float Average
        {
            get;
            set;
        }

            [Inject] public void Install(SamplePoints samplePoints, GasTexture gasTexture, TextureMap textureMap)
        {
            if(enabled)
                samplePoints.Register(this);
            _samplePoints = samplePoints;
            _gasTexture = gasTexture;
            _textureMap = textureMap;
        }

        private void OnEnable()
        {
            if (_samplePoints != null) _samplePoints.Register(this);
        }

        private void OnDisable()
        {
            if(_samplePoints != null) _samplePoints.Unregister(this);
        }

        private void Update()
        {
            if (_textureMap == null) return;
            var position = transform.position;
            InBounds = _textureMap.IsWsPositionInBounds(position);
            Texel = _gasTexture.GetTexelFromWorldPos(position);
        }

        public GasSampleData GetSampleData()
        {
            var pos = transform.position;
            
            return new GasSampleData
            {
               texel = _textureMap.GetTextureCoord(pos),
               gasValue = 0
            };
        }
        
        
        public void SetSampleData(GasSampleData data)
        {
            var pos = transform.position;
            
            
           GasValue = data.gasValue;
           Total = data.gradient.Sum();
           Average = data.gradient.Average();
           MaxValue = data.gradient.Maximum();
           
           if (spriteRenderer != null)
           {
               var color = gradient.Evaluate(data.gradient.Maximum());
               spriteRenderer.color = color;
           }
        }
    }





    public class SamplePoints : Registry<SamplePoint>
    {
        
    }
}