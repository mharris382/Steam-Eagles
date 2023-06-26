using System;
using ObjectLabelMapping;
using UnityEngine;

namespace CoreLib.Audio
{
    public class ParameterLinker : MonoBehaviour
    {
        public FMODLabeledParameter[] parameters;



        private void Start()
        {
            foreach (var fmodLabeledParameter in parameters)
            {
                fmodLabeledParameter.Link();  
            }
        }
    }
}