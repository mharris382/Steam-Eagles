using PhysicsFun;
using UniRx;
using UnityEngine;

namespace Buildings
{
    [RequireComponent(typeof(WallFaderController))]
    public class BuildingFaderTrigger : MonoBehaviour
    {
        private WallFaderController _wallFaderController;
        private StructureState _structureState;

        private void Awake()
        {
            
            this._wallFaderController = this.GetComponent<WallFaderController>();
            this._structureState = this.GetComponentInParent<StructureState>();
           
            var hasAnyPlayers = _structureState.PlayerCountChanged.Select(t => t > 0);
            var fadeIn = hasAnyPlayers.Where(t => !t).DistinctUntilChanged();
            var fadeOut = hasAnyPlayers.Where(t => t).DistinctUntilChanged();
            
            fadeIn.Subscribe(_ => _wallFaderController.FadeIn()).AddTo(this);
            fadeOut.Subscribe(_ => _wallFaderController.FadeOut()).AddTo(this);
        }
    }
}