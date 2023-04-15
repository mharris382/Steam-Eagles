using Buildings.Rooms;
using Tools.BuildTool;
using UnityEngine;

namespace Tools.GenericTools
{
    public class TwoHandedToolController : ToolControllerBase
    {
        public OneHandedToolController leftHand;
        public OneHandedToolController rightHand;


        private void OnEnable()
        {
            leftHand.enabled = false;
            rightHand.enabled = false;
        }
        
        private void OnDisable()
        {
            leftHand.enabled = true;
            rightHand.enabled = true;
        }

        protected override void OnRoomChanged(Room room)
        {
            Debug.LogWarning($"{nameof(TwoHandedToolController)} throw new System.NotImplementedException();");
        }
    }
}