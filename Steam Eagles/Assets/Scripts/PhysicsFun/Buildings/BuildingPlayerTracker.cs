using System;
using System.Collections;
using UnityEngine;

namespace Buildings
{
    public class BuildingPlayerTracker : MonoBehaviour
    {
        public BuildingState buildingState;
        public Building building;
        private BoxCollider2D box;

        private void Awake()
        {
            building = GetComponent<Building>();
            buildingState = GetComponent<BuildingState>();
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
                buildingState.SetPlayerInBuilding(1, true);
            }

            else if (col.CompareTag("Builder"))
            {
                buildingState.SetPlayerInBuilding(0, true);
            }
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (col.CompareTag("Transporter"))
            {
                buildingState.SetPlayerInBuilding(1, false);
            }

            else if (col.CompareTag("Builder"))
            {
                buildingState.SetPlayerInBuilding(0, false);
            }
        }
    }
}