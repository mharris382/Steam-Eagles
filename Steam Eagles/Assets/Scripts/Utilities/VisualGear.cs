using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public class VisualGear : MonoBehaviour
    {
        public List<VisualGear> childGears;


        public int teethCount = 4;
        
        
        public void UpdateChildren(float deltaRotation)
        {
            transform.rotation = Quaternion.Euler(0, 0, transform.eulerAngles.z + deltaRotation);
            deltaRotation *= -1;
            foreach (var child in childGears)
            {
                if (child == null) continue;
                child.UpdateChildren(deltaRotation * GetGearDifference(this, child));
            }
        }
        
        static float GetGearDifference(VisualGear parent, VisualGear child)
        {
            return parent.teethCount / (float)child.teethCount;
        }
    }
}