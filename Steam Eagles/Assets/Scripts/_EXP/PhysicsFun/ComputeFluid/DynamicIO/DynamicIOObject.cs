using System;
using _EXP.PhysicsFun.ComputeFluid.Utilities;
using Buildings;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public class DynamicIOObject : MonoBehaviour
    {
        private static Color sinkColor = new Color(0.8f, 0.1f, 0.2f);
        private static Color sourceColor = new Color(0.1f, 0.8f, 0.2f);

        [SerializeField, Range(-1, 1), GUIColor(nameof(guiColor))]
        private float value = 0.1f;
        
        [OnValueChanged(nameof(ClampSize))]
        public Vector2Int size = new Vector2Int(1, 1);


        #region [Debug]

        [ShowInInspector, BoxGroup("Debug"), ReadOnly]
        public Vector2Int Position
        {
            get;
            set;
        }

        [ShowInInspector, BoxGroup("Debug"), ReadOnly]
        public Vector2Int Size
        {
            get;
            set;
        }
        [ShowInInspector, BoxGroup("Debug"), ReadOnly]
        public float DeltaIn
        {
            get;
            set;
        }

        [ShowInInspector, BoxGroup("Debug"), ReadOnly]
        public float DeltaOut
        {
            get;
            set;
        }

        #endregion
        
        void ClampSize()
        {
            size.x = Mathf.Max(1, size.x);
            size.y = Mathf.Max(1, size.y);
        }
        
        private Color guiColor => value == 0 ? Color.gray : value > 0 ? sourceColor : sinkColor;
        
        
        private Building _building;
        public Building Building => _building ? _building : _building = GetComponentInParent<Building>();

        private TextureMap _textureMap;
        private DynamicIObjects _dynamicIO;

        [Inject] public void Install(DynamicIObjects dynamicIO, TextureMap textureMap)
        {
            _textureMap = textureMap;
            _dynamicIO = dynamicIO;
            if(enabled)
                dynamicIO.Register(this);
           
        }


        private void OnEnable()
        {
            if (_dynamicIO != null) _dynamicIO.Register(this);
        }

        private void OnDisable()
        {
            if(_dynamicIO != null) _dynamicIO.Unregister(this);
        }
        

        public DynamicIOData GetDynamicIOData()
        {
            if (_textureMap == null) return default;
            if (transform == null) return default;
            this.Position = _textureMap.GetTextureCoord(transform.position);
            this.DeltaIn = GetTargetGasIOValue();
            this.Size = new Vector2Int(Mathf.Max(1, this.size.x), Mathf.Max(1, this.size.y));
            return new DynamicIOData()
            {
                deltaIn =DeltaIn,
                position = Position,
                size = Size
            };
        }
        public void SetDynamicIOData(DynamicIOData data)
        {
            OnGasIO(data.deltaOut);
            DeltaOut = data.deltaOut;
        }
        bool HasResources => Building != null;

        public virtual float GetTargetGasIOValue() => value;

        public virtual void OnGasIO(float gasDelta)
        {
            
        }
    }
}