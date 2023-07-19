using UnityEngine;

namespace CoreLib.Signals
{
    public class PCInfo
    {
        public int PlayerNumber { get; private set; }
        public PCInstance PC { get; private set; }
        public IPCTracker PCTracker { get; private set; }

        public GameObject Camera => PC.camera;
        
        public GameObject Input => PC.input;
        
        public GameObject Character => PC.character;
        
        public PCInfo(int playerNumber, PCInstance pc, IPCTracker pcTracker)
        {
            PlayerNumber = playerNumber;
            PC = pc;
            PCTracker = pcTracker;
        }

        public void Reset(int playerNumber, PCInstance pc, IPCTracker tracker)
        {
            PlayerNumber = playerNumber;
            PC = pc;
            PCTracker = tracker;
        }

        public void ResetFrom(PCInfo otherInfo)
        {
            Reset(otherInfo.PlayerNumber, otherInfo.PC, otherInfo.PCTracker);
        }
    }
}