using System;
using Buildables;
using UnityEngine;

namespace Buildings.SaveLoad
{
    [Serializable]
    public class MachineSaveData
    {
        public string machineAddress;
        public Vector2Int machineCellPosition;
        public bool isFlipped;
        public string customSaveData;
        public bool IsValid() => !string.IsNullOrEmpty(machineAddress);
            
        public MachineSaveData() { }

        public MachineSaveData(BuildableMachineBase machine)
        {
            if (String.IsNullOrEmpty(machine.machineAddress))
            {
                throw new InvalidOperationException($"Cannot save machine {machine.name} with no address.");
            }
            machineAddress = machine.machineAddress;
            machineCellPosition = machine.CellPosition;
            isFlipped = machine.IsFlipped;
            var customSaver = machine.GetComponent<IMachineCustomSaveData>();
            if (customSaver != null)
            {
                customSaveData = customSaver.SaveDataToJson();
            }
        }
    }
}