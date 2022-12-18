using System;
using UnityEngine;

namespace Core.SharedVariables
{
    public abstract class SharedVariableAssigner<T, TVariable> : MonoBehaviour where TVariable : SharedVariable<T>
    {
        protected abstract T ResolveValue();
        
        
        [SerializeField] private TVariable _sharedVariable;
        [SerializeField] private AssignmentType assignmentType = AssignmentType.Awake;

        [Flags] enum AssignmentType { 
            Manual = 0b0000,
            Awake = 0b0001, 
            Enable = 0b0010, 
            Start = 0b0100,
        }
        
        private void Awake()
        {
            if (assignmentType== AssignmentType.Awake) AssignValue();
        }

        private void Start()
        {
            if(assignmentType==AssignmentType.Start) AssignValue();
        }

        private void OnEnable()
        {
            if(assignmentType==AssignmentType.Enable) AssignValue();
        }

        
        public void AssignValue() => _sharedVariable.Value = ResolveValue();
    }
}