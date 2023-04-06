using Statuses;
using UnityEngine;

namespace CoreLib.EntityTag
{
    [DisallowMultipleComponent]
    public class Entity : MonoBehaviour
    {
        public EntityType entityType = EntityType.CHARACTER;
    }
}