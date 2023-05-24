using CoreLib.Entities;
using UnityEngine;

namespace Weather.Storms.EntitySubjects
{
    [RequireComponent(typeof(EntityInitializer))]
    public class EntityStormSubject : MonoBehaviour
    {
        private EntityInitializer _entityInitializer;
        public EntityInitializer EntityInitializer => _entityInitializer ??= GetComponent<EntityInitializer>();
    }
}