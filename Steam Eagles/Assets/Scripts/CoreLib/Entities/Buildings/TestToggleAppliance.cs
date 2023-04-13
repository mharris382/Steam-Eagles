using System;
using Buildings.Appliances;
using UnityEngine;

namespace CoreLib.Entities.Buildings
{
    public class TestToggleAppliance : MonoBehaviour
    {
        public KeyCode toggleKey = KeyCode.F3;


        public BuildingAppliance appliance;

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                appliance.IsOn = !appliance.IsOn;
            }
        }
    }
}