using CoreLib;
using UnityEngine;

namespace StateMachine
{
    [CreateAssetMenu(menuName = "Shared Variables/Create Shared Transform", fileName = "SharedTransform", order = 0)]
    public class SharedTransform : SharedVariable<Transform>
    {
        [Tooltip("Optional fallback option used to find desired transform in the scene if it has not bee set.  If tag is empty, it will be ignored.  Primary use case is for when you want to access a transform's value from the editor without playing the scene")]
        public string lookupTag;
        public override Transform Value
        {
            get
            {
                if (base.Value == null && !string.IsNullOrEmpty(lookupTag))
                {
                    var foundGo = GameObject.FindWithTag(lookupTag);
                    if (!foundGo)
                    {
                        Debug.LogWarning($"No object tagged with {lookupTag} found in scene!", this);
                        return null;
                    }
                    base.Value = foundGo.transform;
                }
                return base.Value;
            }
            set => base.Value = value;
        }
    }
}