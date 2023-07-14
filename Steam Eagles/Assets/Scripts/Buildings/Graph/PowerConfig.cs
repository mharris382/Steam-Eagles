using System;

namespace Buildings.Graph
{
    [Serializable]
    public class PowerConfig
    {
        public float updateRate = 0.5f;
        public int maxNodesToProcessPerUpdate = 100;
    }
}