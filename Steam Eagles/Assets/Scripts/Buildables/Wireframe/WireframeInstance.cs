using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildables.Wireframe
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class WireframeInstance : MonoBehaviour
    {
        private SpriteRenderer _sr;
        public SpriteRenderer Sr => _sr!=null ? _sr : _sr = GetComponent<SpriteRenderer>();

        
        [LabelText("BM"), OnValueChanged(nameof(SetMachine))]
        public BuildableMachine machine;


        private void Awake()
        {
            Sr.drawMode = SpriteDrawMode.Sliced;
        }

        public void SetMachine(BuildableMachine bm)
        {
            SetMachine(bm, null);
        }
        public void SetMachine(BuildableMachine bm, GameObject buildingTarget)
        {
            if (buildingTarget== null && bm.buildingTarget == null)
            {
                Debug.LogError("You must specify a building target for the wireframe",this);
                return;
            }
            machine = bm;
            Sr.size = bm.WsSize;
        }
    }
}