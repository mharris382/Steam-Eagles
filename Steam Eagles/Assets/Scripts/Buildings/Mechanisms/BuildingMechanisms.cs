using System;
using System.Collections.Generic;
using Buildings.Mechanisms;
using UniRx;
using UnityEngine;

namespace Buildings
{
    [RequireComponent(typeof(Building))]
    public class BuildingMechanisms : BuildingSubsystem<BuildingMechanism>
    {
     
        
        private Rigidbody2D _rigidbody2D;
        
        public Rigidbody2D Rigidbody2D => _rigidbody2D != null ? _rigidbody2D : _rigidbody2D = GetComponent<Rigidbody2D>();


        
        
        

        
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

        public override void OnSubsystemEntityRegistered(BuildingMechanism entity)
        {
            if (entity is JointedMechanism jointedMechanism)
            {
                jointedMechanism.BuildingJoint.connectedBody = Rigidbody2D;
            }
        }

        public override void OnSubsystemEntityUnregistered(BuildingMechanism entity)
        {
            
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
