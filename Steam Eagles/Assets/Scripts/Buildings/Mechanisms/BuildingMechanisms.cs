using System;
using System.Collections.Generic;
using Buildings.Mechanisms;
using UniRx;
using UnityEngine;

namespace Buildings
{
    [RequireComponent(typeof(Building))]
    public class BuildingMechanisms : MonoBehaviour
    {
        private Building _building;

        public Building Building => _building != null ? _building : _building = GetComponent<Building>();
        
        private Rigidbody2D _rigidbody2D;
        
        public Rigidbody2D Rigidbody2D => _rigidbody2D != null ? _rigidbody2D : _rigidbody2D = GetComponent<Rigidbody2D>();


        private ReactiveCollection<BuildingMechanism> _mechanisms = new ReactiveCollection<BuildingMechanism>();
        
        
        
        public void RegisterMechanism(BuildingMechanism buildingMechanism)
        {
            Debug.Log($"Registering {buildingMechanism.name} on {name}");
            _mechanisms.Add(buildingMechanism);
            if (buildingMechanism is JointedMechanism jointedMechanism)
            {
                jointedMechanism.BuildingJoint.connectedBody = Rigidbody2D;
            }
        }
        
        public void UnregisterMechanism(BuildingMechanism buildingMechanism)
        {
            Debug.Log($"Unregistering {buildingMechanism.name} on {name}");
            _mechanisms.Remove(buildingMechanism);
        }


        
        /// <summary>
        /// jointed mechanisms will normally be linked at runtime when registered, but this method can be used to link them immediately for use in the editor
        /// </summary>
        public void LinkJointsImmediately()
        {
            foreach (var jointed in GetComponentsInChildren<JointedMechanism>())
            {
                jointed.BuildingJoint.connectedBody = Rigidbody2D;
            }
        }
    }


    [Serializable]
    public class MechanismSaves
    {
        
    }

    [Serializable]
    public class MechanismSave
    {
        public float[] mechanismSaveState;
    }
}
