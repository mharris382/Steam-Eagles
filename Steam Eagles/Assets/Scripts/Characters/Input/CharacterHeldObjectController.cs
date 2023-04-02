using System;
using UnityEngine;

namespace Characters
{
    [System.Obsolete("This will be replaced with the inventory/tool system")]
    public class CharacterHeldObjectController : MonoBehaviour
    {
        [SerializeField] TriggerArea inRangePickups;


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                switch (inRangePickups.GetTargetCount())
                {
                    case 0:
                        break;
                    case 1:
                        Pickup(inRangePickups.GetTarget(0));
                        break;
                    default:
                        Pickup(inRangePickups.GetNearestTarget());
                        break;
                }
            }
        }

        private void Pickup(Rigidbody2D getTarget)
        {
            throw new NotImplementedException();
        }
    }
}