using CoreLib.MyEntities;
using UnityEngine;

namespace Weather.Storms
{
    public interface IStormSubject
    {
        public EntityType SubjectEntityType { get; }
        
        public Bounds SubjectBounds { get; }
        
        public void OnStormAdded(Storm storm);
        public void OnStormRemoved(Storm storm);
    }
}