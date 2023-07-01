using System;
using System.Collections;
using AI.Enemies;
using UnityEngine;

namespace PhysicsFun
{
    [System.Serializable]
    public class FireSpreadConfig
    {
        public float spreadRate = 2;
    }
    public class FireSpreadHandler
    {
        
    }
    
    public class FireSpreader : MonoBehaviour
    {
        public GroundCheckConfig rightSideCheck;
        public GroundCheckConfig leftSideCheck;

        public float spreadRate = 2;

        

        IEnumerator DoFireSpread()
        {
            while (enabled)
            {
                yield return null;
            }
        }

        void DoSpread(float direction, ref Vector3 position)
        {
            
        }
        
        
    }
}