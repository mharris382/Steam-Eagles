using System;

namespace Buildings
{
    [Serializable]
    public class ElectricityConfig
    {
        public float connectionRate = 5;
        public int connectRadius = 5;
        public bool alwaysConnectConsumers = true;
    }
}