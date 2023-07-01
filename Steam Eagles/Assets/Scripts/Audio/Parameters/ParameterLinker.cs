using System;
using ObjectLabelMapping;
using UnityEngine;

namespace CoreLib.Audio
{
    /// <summary>
    /// place this in scene to link parameters. doing so will allow you to implicitly reference parameter based
    /// on the assets in the FMODLabeledParameter array
    /// </summary>
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