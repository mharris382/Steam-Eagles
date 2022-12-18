using System;
using UnityEngine;

namespace Core.SharedVariables
{
    public abstract class SharedVariableToggleAssigner<T, TVariable> : MonoBehaviour where TVariable : SharedVariable<T>
    { 
        protected abstract T ResolveValue(bool isOn);
        
        [SerializeField] private TVariable _sharedVariable;
        [SerializeField] private AssignmentType assignmentType = AssignmentType.EnableDisable;
        [Flags] enum AssignmentType { 
            Manual = 0b0000,
            AwakeDestroy = 0b0001,
            EnableDisable = 0b0010,
        }

        private void Awake()
        {
            if (assignmentType == AssignmentType.AwakeDestroy)
            {
                AssignValue(true);   
            }
        }
        private void OnDestroy()
        {
            if (assignmentType == AssignmentType.AwakeDestroy)
            {
                AssignValue(false);   
            }
        }
        private void OnEnable()
        {
            if (assignmentType == AssignmentType.EnableDisable)
            {
                AssignValue(true);   
            }
        }
        private void OnDisable()
        {
            if (assignmentType == AssignmentType.EnableDisable)
            {
                AssignValue(false);   
            }
        }
        
        public void AssignValue(bool isOn) => _sharedVariable.Value = ResolveValue(isOn);
    }
}