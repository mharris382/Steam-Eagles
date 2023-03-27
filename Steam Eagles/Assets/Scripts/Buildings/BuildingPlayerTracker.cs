using System;
using System.Collections;
using PhysicsFun.Buildings;
using UnityEngine;

namespace Buildings
{
    public class BuildingPlayerTracker : MonoBehaviour
    {
        public StructureState structureState;
        public Building building;
        private BoxCollider2D box;

        private void Awake()
        {
            building = GetComponent<Building>();
            structureState = GetComponent<StructureState>();
            this.box= gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = building.sizeWorldSpace.size;
            box.offset = transform.InverseTransformPoint(building.sizeWorldSpace.center);
        }
        
        

        private IEnumerator Start()
        {
            box.enabled = false;
            yield return null;
            yield return null;
            yield return null;
            box.enabled = true;
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Transporter"))
            {
                structureState.SetPlayerInBuilding(1, true);
            }

            else if (col.CompareTag("Builder"))
            {
                structureState.SetPlayerInBuilding(0, true);
            }
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (col.CompareTag("Transporter"))
            {
                structureState.SetPlayerInBuilding(1, false);
            }

            else if (col.CompareTag("Builder"))
            {
                structureState.SetPlayerInBuilding(0, false);
            }
        }
    }
}