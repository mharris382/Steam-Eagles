using UnityEngine;

namespace Items.SaveLoad
{
    public class ToolBeltInventory : CharacterInventoryBase
    {
        [SerializeField] private InventorySlot recipeToolSlot;
        [SerializeField] private InventorySlot buildToolSlot;
        [SerializeField] private InventorySlot repairToolSlot;
        [SerializeField] private InventorySlot destructToolSlot;
        
        
        
        public override string UniqueInventoryID => "ToolBelt";
        public override int NumberOfSlots => 4;

        public override void InitSlots()
        {
            Debug.Assert(recipeToolSlot != null, "No Recipe tool slot");
            Debug.Assert(buildToolSlot != null, "No build tool slot");
            Debug.Assert(repairToolSlot != null, "No repair tool slot");
            Debug.Assert(destructToolSlot != null, "No destruct tool slot");   
        }

        public override InventorySlot GetSlotFor(int slotNumber)
        {
            switch (slotNumber)
            {
                case 0:
                    return recipeToolSlot;
                case 1:
                    return buildToolSlot;
                case 2:
                    return repairToolSlot;
                case 3:
                    return destructToolSlot;
                default:
                    return null;
            }
        }
    }
}