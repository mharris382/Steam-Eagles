using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreLib.Audio;
using ObjectLabelMapping;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using Zenject;

namespace CoreLib.Audio
{
    [GlobalConfig("Resources/Audio/ParameterDatabase")]
    public class FMODParameterDatabase : GlobalConfig<FMODParameterDatabase>
    {
        [AssetList(AutoPopulate = true)]
        public List<FMODLabeledParameter> parameters = new List<FMODLabeledParameter>();
        
        
        
        public class Linker : IInitializable
        {
            private readonly FMODParameterDatabase _parameterDatabase;
            public Linker(FMODParameterDatabase parameterDatabase)
            {
                _parameterDatabase = parameterDatabase;
            }
            public void Initialize()
            {
                var parameters = from parameter in _parameterDatabase.parameters where parameter.IsValid() select parameter;
                foreach (var parameter in parameters) parameter.Link();
            }
        }



        private FMODLabeledParameter GetParameter(string parameter)
        {
            return parameters.FirstOrDefault(p => p.name == parameter);
        }
        public static ValueDropdownList<string> GetParameters()
        {
            var vdl = new ValueDropdownList<string>();
            foreach (var fmodLabeledParameter in Instance.parameters)
            {
                if (fmodLabeledParameter.IsValid())
                    vdl.Add(fmodLabeledParameter.name);
            }
            return vdl;
        }
        public static ValueDropdownList<string> GetLabelsFor(string parameter)
        {
            var vdl = new ValueDropdownList<string>();
            var p = Instance.GetParameter(parameter);
            if (p == null)
                return vdl;
            foreach (var label in p.labels)
            {
                if(!string.IsNullOrEmpty(label))
                    vdl.Add(label);
            }
            return vdl;
        }
    }
}
