using _EXP.PhysicsFun.ComputeFluid.Utilities;
using UnityEngine;
using Zenject;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public abstract class DynamicForceInputObject : MonoBehaviour
    {
        private DynamicForceInputObjects _dynamicInputs;
        private TextureMap _textureMap;

        [SerializeField] private Vector2Int size = new Vector2Int(5, 5);

        [Inject]
        public void Install(DynamicForceInputObjects dynamicInputs, TextureMap textureMap)
        {
            _textureMap = textureMap;
            _dynamicInputs = dynamicInputs;
            if (enabled) dynamicInputs.Register(this);
        }

        private void OnEnable() => _dynamicInputs?.Register(this);

        private void OnDisable() => _dynamicInputs?.Unregister(this);

        public Vector2Int Texel => _textureMap == null ? default : _textureMap.GetTextureCoord(transform.position);
        public Vector2Int Size => size;

        public abstract Vector2 GetInputForce();

        public DynamicForceInput GetForceData()
        {
            return new DynamicForceInput()
            {
                texel = Texel,
                area = Size,
                velocity = GetInputForce()
            };
        }
    }
}