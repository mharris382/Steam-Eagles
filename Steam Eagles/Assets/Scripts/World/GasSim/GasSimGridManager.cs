using System.Collections.Generic;
using CoreLib;
using UnityEngine;

namespace GasSim
{
    public class GasSimGridManager : Singleton<GasSimGridManager>
    {
        Dictionary<Grid, IGasSim> _gridToGasSim = new Dictionary<Grid, IGasSim>();
        Dictionary<IGasSim, Grid> _gasSimToGrid = new Dictionary<IGasSim, Grid>();
        
        
        public void RegisterSim(Grid grid, IGasSim gasSim)
        {
            _gridToGasSim.Add(grid, gasSim);
            _gasSimToGrid.Add(gasSim, grid);
        }
        
        public IGasSim GetGasSim(Grid grid)
        {
            return _gridToGasSim[grid];
        }
    }
}