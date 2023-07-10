using _EXP.PhysicsFun.ComputeFluid.Utilities;
using UnityEngine;
using Zenject;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public class GasBridge : MonoBehaviour
    {
        private GasTexture _gasTexture;
        private SamplePoints _samplePoints;
        private TextureMap _textureMap;
        private DynamicIObjects _dynamicIObjects;


        [Inject]
        void Install(GasTexture gasTexture, 
            SamplePoints samplePoints, 
            TextureMap textureMap,
            DynamicIObjects dynamicIObjects, 
            SamplePointHandle.Factory samplePointHandleFactory)
        {
            
            _gasTexture = gasTexture;
            _samplePoints = samplePoints;
            _textureMap = textureMap;
            _dynamicIObjects = dynamicIObjects;
        }
        
        
        
    }
}