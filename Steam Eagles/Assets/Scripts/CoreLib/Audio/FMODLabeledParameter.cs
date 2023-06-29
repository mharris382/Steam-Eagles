using System;
using System.Collections.Generic;
using System.Linq;
using FMOD;
using FMOD.Studio;
using ObjectLabelMapping;
using Sirenix.OdinInspector;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif
namespace CoreLib.Audio
{
    
    [CreateAssetMenu(fileName = "New Audio Parameter", menuName = "Steam Eagles/Audio/FMOD Labeled Parameter",
        order = -15)]
    public class FMODLabeledParameter : SerializedScriptableObject, IParameterMapProvider
    {
        public FMODUnity.EventReference fmodEvent;

        [ValidateInput(nameof(ValidateParameterName), "Parameter name cannot be empty")] public string parameterName;
      
        [ValidateInput(nameof(ValidateLabels), "Labels cannot be empty")] [ListDrawerSettings()] [SerializeField] internal string[] labels;
        [ToggleGroup(nameof(useAdditionalParameters))] public bool useAdditionalParameters;
        [ToggleGroup(nameof(useAdditionalParameters))] public string[] additionalParameters;
        
        [ShowIf(nameof(IsValid)),ListDrawerSettings(CustomAddFunction = nameof(CreateNew)),SerializeField] private MappedObject[] mappedObjects;

        
         
        [Serializable]
        class MappedObject : IParameterMappedObject
        {
            
            [SerializeField, ReadOnly] private FMODLabeledParameter labeledParameter;

            
            [HorizontalGroup("Label",Width = 150,LabelWidth =0), HideLabel]
            [SerializeField, PreviewField(ObjectFieldAlignment.Left, Height = 150), TableColumnWidth(150, false), Required] private UnityEngine.Object linkedObject;
            
            [HorizontalGroup("Label",LabelWidth =0)]
            [VerticalGroup("Label/Label")]
            [SerializeField,ValueDropdown(nameof(Labels), DropdownHeight = 150), TableColumnWidth(50), HideLabel] private string label;
             
            
            [VerticalGroup("Label/Label")]
            [ToggleGroup("Label/Label/multipleObjects")]
            [SerializeField, ToggleLeft, LabelText("Multi?")]
            private bool multipleObjects;
            
            [ShowIf("multipleObjects")]
            [ToggleGroup("Label/Label/multipleObjects")]
            [SerializeField, PreviewField,HideLabel]
            private UnityEngine.Object[] linkedObjects;
            
            
           
           

            public string Label => label;
            public UnityEngine.Object LinkedObject => linkedObject;
            public string ParameterName => labeledParameter.parameterName;
            public MappedObject(FMODLabeledParameter labeledParameter)
            {
                this.labeledParameter = labeledParameter;
                this.label = labeledParameter.labels[0];
            }
            
            
            public IEnumerable<Object> LinkedObjects()
            {
                if (multipleObjects)
                {
                    foreach (var o in linkedObjects)
                    {
                        if(o != null)
                            yield return o;
                    }
                }
                if(linkedObject != null)
                    yield return linkedObject;
            }

            #region [Editor Helpers]

            ValueDropdownList<string> Labels()
            {
                var vdl = new ValueDropdownList<string>();
                foreach (var label in labeledParameter.labels)
                    vdl.Add(label);
                return vdl;
                
            }

            #endregion
            
            public IEnumerable<IParameterMappedObject> GetParameters()
            {
                yield return this;
                if (this.multipleObjects)
                {
                    foreach (var o in linkedObjects)
                    {
                        yield return new ParameterMappedObject(ParameterName, label, o);
                    }
                }
                if (labeledParameter.useAdditionalParameters)
                {
                    foreach (var labeledParameterAdditionalParameter in labeledParameter.additionalParameters)
                    {
                        yield return new ParameterMappedObject(labeledParameterAdditionalParameter, label, linkedObject);
                        if (this.multipleObjects)
                        {
                            foreach (var o in linkedObjects)
                            {
                                yield return new ParameterMappedObject(labeledParameterAdditionalParameter, label, o);
                            }
                        }
                    }
                }
            }
        }
        
        
        
        
        private string GetLabel(string label) => labels[GetLabelIndex(label)];


        private int GetLabelIndex(string label)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] == label)
                    return i;
            }
            return -1;
        }


        public bool SetLabel(EventInstance eventInstance, string label)
        {
            var res = eventInstance.setParameterByNameWithLabel(parameterName, GetLabel(label));
            if (res != RESULT.OK) Debug.LogError($"Failed to set parameter {parameterName} to {label} on event ", this);
            return res == RESULT.OK;
        }

        public IEnumerable<IParameterMappedObject> GetParameters() => mappedObjects.SelectMany(t => t.GetParameters());


        #region [Editor Helpers]

        #region [Validation]

        private bool IsValid() => ValidateParameterName(parameterName) && ValidateLabels(labels);


        private bool ValidateParameterName(string parameterName)
        {
            if(string.IsNullOrEmpty(parameterName)){
                return false;
            }
            return true;
        }

        private bool ValidateLabels(string[] labels)
        {
            if(labels.Length == 0){
                return false;
            }

            foreach (var label in labels)
            {
                if (string.IsNullOrEmpty(label))
                    return false;
            }
            return true;
        }

        #endregion

        private MappedObject CreateNew() => new MappedObject(this);

        #endregion
    }
}