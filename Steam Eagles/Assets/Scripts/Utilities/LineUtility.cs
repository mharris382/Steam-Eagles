using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utilities
{
    public class LineUtility : MonoBehaviour
    {
        public LineRenderer Line => _lr ? _lr : _lr = GetComponent<LineRenderer>();
        private LineRenderer _lr;

        public void UpdatePoints(IEnumerable<Vector3> newPoints)
        {
            var pnts = newPoints.ToArray();
            Line.positionCount = pnts.Length;
            Line.SetPositions(pnts);
        }
    }
}