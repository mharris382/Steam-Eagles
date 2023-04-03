using UnityEngine;

namespace Buildings.Mechanisms
{
    public abstract class BuildingMechanism : BuildingSubsystemEntity<BuildingMechanisms, BuildingMechanism>, IEntityID
    {
        private BuildingMechanisms _building;
        public BuildingMechanisms BuildingMechanisms => _building != null ? _building : _building = GetComponentInParent<BuildingMechanisms>();

        void Awake()
        {
            if (BuildingMechanisms == null)
            {
                Debug.LogError($"No BuildingMechanisms found on {name}");
            }
        }
        protected virtual void Start()
        {
            BuildingMechanisms.RegisterSubsystemEntity(this);
        }


        private void OnDestroy()
        {
            BuildingMechanisms.UnregisterSubsystemEntity(this);
        }

        public abstract float[] GetSaveState();

        public abstract void LoadSaveState(float[] saveState);

    }
    
    
    public abstract class JointedMechanism : BuildingMechanism
    {
        /// <summary>
        /// joint which is used to attach the mechanism to the building
        /// </summary>
        public abstract Joint2D BuildingJoint { get; }
    }
}