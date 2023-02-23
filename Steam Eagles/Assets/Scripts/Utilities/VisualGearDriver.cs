using UnityEngine;

namespace Utilities
{
    [ExecuteInEditMode]
    [ExecuteAlways]
    public class VisualGearDriver : VisualGear
    {
        public float rotationSpeed;
        
        
        void Update()
        {
            
            this.UpdateChildren(rotationSpeed * Time.deltaTime);
        }
    }
}