using CoreLib;
using UnityEngine;

namespace GasSim
{
    [RequireComponent(typeof(Grid))]
    public class GlobalGasGrid : Singleton<GlobalGasGrid>
    {
        private Grid _grid;
        public Grid Grid => _grid ? _grid : _grid = GetComponent<Grid>();
        public override bool DestroyOnLoad => true;
    }
}