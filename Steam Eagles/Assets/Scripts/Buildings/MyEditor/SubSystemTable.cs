using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildings.MyEditor
{
    public abstract class SubSystemTable<TSubsystem, TEntity> : IBuildingTable
        where TSubsystem : BuildingSubsystem<TEntity>
        where TEntity : Component, IEntityID
    {
        protected readonly Building _building;
        public bool IsValid => _building != null;
       
        
        [ShowInInspector]
        public readonly TSubsystem _buildingSubsystem;
        
        [Button]
        void RefreshTable()
        {
            _wrappers = new List<SubsystemEntityWrapper<TSubsystem, TEntity>>();
            InitializeWrappers(_wrappers);
        }
        [ValidateInput(nameof(ValidateWrappers))]
        [ShowInInspector]
        [TableList(IsReadOnly = true, AlwaysExpanded = true)]
        protected List<SubsystemEntityWrapper<TSubsystem, TEntity> > _wrappers = new List<SubsystemEntityWrapper<TSubsystem, TEntity> >();


        bool ValidateWrappers(List<SubsystemEntityWrapper<TSubsystem, TEntity>> wrappers, ref string error)
        {
            HashSet<string> guids = new HashSet<string>();
            foreach (var wrapper in wrappers)
            {
                if (guids.Contains(wrapper._entity.GetEntityGUID()))
                {
                    error = $"Duplicate GUID: {wrapper._entity.GetEntityGUID()}";
                    return false;
                }
                guids.Add(wrapper._entity.GetEntityGUID());
            }

            return true;
        }
        
        [GUIColor(nameof(GetWarningColor))]
        [ShowIf(nameof(HasParentingIssue))]
        [Button(ButtonSizes.Medium)]
        void ReParentEntities()
        {
            foreach (var wrapper in _wrappers)
            {
                wrapper._entity.transform.SetParent(_buildingSubsystem.entityParent);
            }
        }
        
        
        bool HasParentingIssue()
        {
            foreach (var wrapper in _wrappers)
            {
                if (wrapper._entity.transform.parent != _buildingSubsystem.entityParent)
                {
                    return true;
                }
            }

            return false;
        }
        
        bool HasDuplicateGUIDIssue()
        {
            HashSet<string> guids = new HashSet<string>();
            foreach (var wrapper in _wrappers)
            {
                if (guids.Contains(wrapper._entity.GetEntityGUID()))
                {
                    return true;
                }
                guids.Add(wrapper._entity.GetEntityGUID());
            }

            return false;
        }
        
        Color GetWarningColor() => Color.Lerp(Color.yellow, Color.white, 0.4f);
        Color GetErrorColor() => Color.Lerp(Color.red, Color.white, 0.4f);
        
        [GUIColor(nameof(GetErrorColor))]
        [Button(ButtonSizes.Large), ShowIf(nameof(HasDuplicateGUIDIssue))]
        public void FixDuplicateGUIDIssue()
        {
            Dictionary<string, int> nameCounts = new Dictionary<string, int>();
            Dictionary<string, List<TEntity>> nameToEntities = new Dictionary<string, List<TEntity>>();
            foreach (var wrapper in _wrappers)
            {

                var guid = wrapper._entity.GetEntityGUID();
                if (!nameToEntities.ContainsKey(guid))
                {
                    nameToEntities.Add(guid, new List<TEntity>());
                }
                nameToEntities[guid].Add(wrapper._entity);
                
                if (!nameCounts.ContainsKey(guid))
                {
                    nameCounts.Add(guid, 0);
                }
                nameCounts[guid]++;
            }

            foreach (var name in nameCounts.Keys)
            {
                if (nameCounts[name] > 1)
                {
                    var entities = nameToEntities[name];
                    for (int i = 0; i < entities.Count; i++)
                    {
                        var entity = entities[i];
                        entity.name = $"{name} ({i})";
                    }
                }
            }
        }
        
        public SubSystemTable(Building building)
        {
            _building = building;
            _buildingSubsystem = _building.GetComponent<TSubsystem>();
            if (_buildingSubsystem == null)
            {
                _buildingSubsystem = _building.gameObject.AddComponent<TSubsystem>();
                
            }

            if (_buildingSubsystem.entityParent == null)
            {
                var p = new GameObject($"[{typeof(TEntity).Name.Replace("Building", "")}s]").transform;
                p.transform.SetParent(_building.transform);
                _buildingSubsystem.entityParent = p;
                p.localPosition = Vector3.zero;
            }
            _wrappers = new List<SubsystemEntityWrapper<TSubsystem, TEntity> >();
            // ReSharper disable once VirtualMemberCallInConstructor
            InitializeWrappers(_wrappers);
        }

        public abstract void InitializeWrappers(List<SubsystemEntityWrapper<TSubsystem, TEntity> > wrappers);
    }
}