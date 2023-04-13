using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Buildings.Appliances
{
    public abstract class BuildingAppliance : MonoBehaviour, IEntityID
    {
        private BuildingAppliances _building;
        public BuildingAppliances BuildingMechanisms => _building != null ? _building : _building = GetComponentInParent<BuildingAppliances>();

        [SerializeField]
        private bool isAlwaysPowered;
        
        public bool IsEntityInitialized { get; set; }
        
        private BoolReactiveProperty _isPowered = new BoolReactiveProperty();
        private BoolReactiveProperty _isApplianceOn = new BoolReactiveProperty();
        public IReadOnlyReactiveProperty<bool> IsApplianceOn => _isApplianceOn;

        
        [Button, ButtonGroup(), DisableInEditorMode]
        void SetOn()=> _isApplianceOn.Value = true;
        [Button, ButtonGroup(), DisableInEditorMode]
        void SetOff() => _isApplianceOn.Value = false;
        
        [ShowInInspector, ReadOnly]
        public bool IsPowered
        {
            get => _isPowered.Value || isAlwaysPowered;
            set => _isPowered.Value = value || isAlwaysPowered;
        }

        [ShowInInspector, ReadOnly]
        public bool IsOn
        {
            get =>  _isApplianceOn.Value;
            set => _isApplianceOn.Value = value;
        }
        
        public string GetEntityGUID()
        {
            return gameObject.name;
        }
    }
}