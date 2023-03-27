using System.Collections.Generic;
using CoreLib;
using UnityEngine;

namespace PhysicsFun.Buildings
{
    public class BuildingManager : Singleton<BuildingManager>
    {
        private Dictionary<string, Building> _registeredBuildings = new Dictionary<string, Building>();
        
        
    }
}