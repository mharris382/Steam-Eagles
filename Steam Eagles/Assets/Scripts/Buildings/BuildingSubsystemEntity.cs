using UnityEngine;

namespace Buildings
{
    public abstract class BuildingSubsystemEntity<TSubsystem, TEntity> : MonoBehaviour where TSubsystem : BuildingSubsystem<TEntity> where TEntity : Component, IEntityID
    {
        private TSubsystem _buildingSubsystem;
        public TSubsystem BuildingSubsystem => _buildingSubsystem != null ? _buildingSubsystem : _buildingSubsystem = GetComponentInParent<TSubsystem>();
        public string GetEntityGUID()
        {
            return gameObject.name;
        }
        
        protected virtual void Start()
        {
            BuildingSubsystem.RegisterSubsystemEntity(this as TEntity);
        }
        
        private void OnDestroy()
        {
            BuildingSubsystem.UnregisterSubsystemEntity(this as TEntity);
        }
    }
}