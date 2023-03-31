using Sirenix.OdinInspector;
using UnityEngine;

namespace Damage
{
    public class DamageManager : MonoBehaviour
    {
        [SceneObjectsOnly, Required] public GameObject structureTarget;
        [Required] public DamageController controller;


        bool IsValid() => controller != null && structureTarget != null;
        
        
        
        public void BeginDamageEvent()
        {
            if (!IsValid())
            {
                return;
            }
            controller.enabled = true;
        }

        public void EndDamageEvent()
        {
            if (!IsValid())
            {
                return;
            }
            controller.enabled = false;
        }
    }
}