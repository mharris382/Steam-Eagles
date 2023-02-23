using System;
using System.Linq;
using UnityEngine;

namespace Items
{
    public class ToolSlotsTester : MonoBehaviour
    {
        public ToolSlots toolSlots;


        private void Awake()
        {
            toolSlots.onToolEquipped.AddListener(t =>
            {
                Debug.Log($"Equipped <b>{t.name}</b>");
            });
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                toolSlots.EquipNextTool();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                toolSlots.EquipPrevTool();
            }
            else
            {
                var tools = toolSlots.GetEquippableTools().ToArray();
                for (int i = 0; i < tools.Length; i++)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    {
                        toolSlots.EquipTool(tools[i]);
                    }
                }
            }
        }
    }
}