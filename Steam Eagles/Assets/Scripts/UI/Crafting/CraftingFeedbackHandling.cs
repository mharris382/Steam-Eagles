using Buildings;
using UI.Crafting.Events;
using UnityEngine;
using Zenject;

namespace UI.Crafting
{
    public class CraftingFeedbackHandling : MonoBehaviour
    {
        
        public GameObject craftingEffect;
        
        private CraftingAimHanding _aimHanding;
        private CraftingBuildingTarget _buildingTarget;


        [Inject]
        void Install(CraftingBuildingTarget buildingTarget, CraftingAimHanding aimHanding)
        {
            _buildingTarget = buildingTarget;
            _aimHanding = aimHanding;
        }


        private void Update()
        {
            var aimWS = _aimHanding.AimWorldSpace;
            if(_buildingTarget.BuildingTarget == null)return;
            var room = _buildingTarget.BuildingTarget.Map.GetRoomWS(aimWS.Value);
            if(room == null)return;
            craftingEffect.transform.SetParent(room.transform);
            craftingEffect.transform.localPosition = Vector3.zero;
            craftingEffect.SendMessage("ChangeRooms", room);
        }
    }
}