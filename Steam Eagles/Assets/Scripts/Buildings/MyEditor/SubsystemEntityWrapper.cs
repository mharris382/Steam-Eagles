using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Buildings.MyEditor
{
    public class SubsystemEntityWrapper<TSubsystem,TEntity> 
        where TEntity : Component, IEntityID  
        where TSubsystem : BuildingSubsystem<TEntity>
    {
        internal  readonly SubSystemTable<TSubsystem, TEntity> _table;
        internal  readonly TEntity _entity;

        [ShowInInspector]
        public string GUID
        {
            get => _entity.GetEntityGUID();
        }

        [ShowInInspector]
        public string EntityType
        {
            get
            {
                return _entity.GetType().Name;
            }
        }


        [Button(ButtonSizes.Medium)]
        void Select()
        {
            Selection.activeGameObject = _entity.gameObject;
        }
        

        public SubsystemEntityWrapper(SubSystemTable<TSubsystem, TEntity> table, TEntity entity)
        {
            _table = table;
            _entity = entity;
        }
    }
}