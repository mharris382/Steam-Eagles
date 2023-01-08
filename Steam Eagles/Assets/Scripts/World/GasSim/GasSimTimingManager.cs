using CoreLib;
using StateMachine;
using UnityEngine;

namespace GasSim
{
    public class GasSimTimingManager : MonoBehaviour
    {
        [Range(0.0001f, 2)]
        public float maxUpdateRate = 0.125f;
        
    }
}