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
        

        [BoxGroup("Parameter Declaration")]  [ValidateInput(nameof(ValidateParameterName), "Parameter name cannot be empty")] public string parameterName;
        //[BoxGroup("Parameter Declaration")]  [ValidateInput(nameof(ValidateLabels), "Labels cannot be empty")] [ListDrawerSettings(ShowFoldout = false)] [SerializeField] 
        internal string[] labels => GetLabels();
        
        
        
        
        //[ToggleGroup(nameof(useAdditionalParameters))] public bool useAdditionalParameters;
        //[ToggleGroup(nameof(useAdditionalParameters))] public string[] additionalParameters;
        
        [ShowIf(nameof(IsValid)),ListDrawerSettings(CustomAddFunction = nameof(CreateNew)),SerializeField] private MappedObject[] mappedObjects;

        [BoxGroup("Parameter Declaration")]
        [Tooltip("Comma Separated List of Labels")]
        [SerializeField, LabelText("Labels List"), DelayedProperty] private string labelsString;


        private string[] GetLabels()
        {
            if (string.IsNullOrEmpty(labelsString))
                return new string[0];
            var list = labelsString.Split(',').Select(s => s.Trim()).ToArray();
            return list;
        }
         
        [Serializable]
        class MappedObject : IParameterMappedObject
        {
            
            [SerializeField, ReadOnly] private FMODLabeledParameter labeledParameter;

            
            [HorizontalGroup("Label",Width = 150,LabelWidth =0), HideLabel]
            [SerializeField, PreviewField(ObjectFieldAlignment.Left, Height = 150), TableColumnWidth(150, false), Required] private UnityEngine.Object linkedObject;
            
            [HorizontalGroup("Label",LabelWidth =0)]
            [VerticalGroup("Label/Label")]
            [SerializeField,ValueDropdown(nameof(Labels), DropdownHeight = 150), TableColumnWidth(50), HideLabel] private string label;


            private bool multipleObjects => linkedObjects != null && linkedObjects.Length > 0;
            
            [VerticalGroup("Label/Label/Details")]
            [SerializeField, PreviewField,HideLabel, ListDrawerSettings(NumberOfItemsPerPage = 4)]
            private UnityEngine.Object[] linkedObjects;
            
            
            [HideLabel, InfoBox("Usage Description"), Tooltip("Explain how these objects will are related to the parameter and how they will be used in the game.")]
            [HorizontalGroup("Label/Label/Details")]
            [ToggleGroup("Label/Label")] [SerializeField, MultiLineProperty(4)]
            private string usageDescription;
           

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

        public bool IsValid() => ValidateParameterName(parameterName) && ValidateLabels(labels);


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