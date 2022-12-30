using Buildings;
using UniRx;
using UnityEngine;

namespace PhysicsFun
{
    [RequireComponent(typeof(WallFaderController))]
    public class BuildingFaderTrigger : MonoBehaviour
    {
        private WallFaderController _wallFaderController;
        private BuildingState _buildingState;

        private void Awake()
        {
            
            this._wallFaderController = this.GetComponent<WallFaderController>();
            this._buildingState = this.GetComponentInParent<BuildingState>();
            
            var hasAnyPlayers = _buildingState.PlayerCountChanged.Select(t => t > 0);
            var fadeIn = hasAnyPlayers.Where(t => !t).DistinctUntilChanged();
            var fadeOut = hasAnyPlayers.Where(t => t).DistinctUntilChanged();
            
            fadeIn.Subscribe(_ => _wallFaderController.FadeIn()).AddTo(this);
            fadeOut.Subscribe(_ => _wallFaderController.FadeOut()).AddTo(this);
        }
    }
}