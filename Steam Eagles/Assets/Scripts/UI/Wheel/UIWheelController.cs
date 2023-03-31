﻿using Players;
using UnityEngine;

namespace UI.Wheel
{
    public class UIWheelController : MonoBehaviour
    {
        public UIWheel wheel;
        public Player player;
        

        bool HasResources()
        {
            if(wheel==null)return false;
            if (player == null) return false;
            if(player.InputWrapper == null) return false;
            if (player.InputWrapper.PlayerInput == null) return false;
            return true;
        }

        private void Update()
        {
            if (!HasResources())
            {
                CloseWheel();
                return;
            }

            var input = player.InputWrapper.PlayerInput;
            if (IsWheelOpen())
            {
                if(input.actions["Open Wheel"].ReadValue<float>()<0.1f)
                {
                    CloseWheel();
                }
            }
            else
            {
                if(input.actions["Open Wheel"].ReadValue<float>()>0)
                {
                    OpenWheel();
                }
            }
           
        }

        bool IsWheelOpen()
        {
            return wheel.enabled;
        }

        void OpenWheel()
        {
            wheel.enabled = true;
            wheel.UpdateChildImages();
            if (player.InputWrapper == null) return;
            var input = player.InputWrapper.PlayerInput;
            input.SwitchCurrentActionMap("Wheel");
            Debug.Assert(input.currentActionMap.name == "Wheel", $"Did not switch to wheel action map! Current ActionMap = {input.currentActionMap.name}", this);
        }

        void CloseWheel()
        {
            wheel.enabled = false;
            if (player.InputWrapper == null) return;
            var input = player.InputWrapper.PlayerInput;
            if(input.currentActionMap.name == "Wheel")
                input.SwitchCurrentActionMap("Gameplay");
        }
    }
}