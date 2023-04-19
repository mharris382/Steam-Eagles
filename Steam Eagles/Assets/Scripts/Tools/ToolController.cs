using System;
using Characters;
using Items;
using SteamEagles.Characters;
using Tools.BuildTool;
using UnityEngine;

namespace Tools
{
    public class ToolController : MonoBehaviour
    {
        private CharacterState _character;
        private ToolState _toolState;

        public float toolSelectionResetTime = 0.125f;
        public CharacterState CharacterState => _character ? _character : (_character = GetComponentInParent<CharacterState>());
        public ToolState ToolState => _toolState ? _toolState : (_toolState = GetComponentInParent<ToolState>());


        private ToolControllerSharedData _toolData;
        public ToolControllerSharedData ToolData => _toolData ? _toolData : (_toolData = GetComponentInParent<ToolControllerSharedData>());


        private void Awake()
        {
            _character = GetComponentInParent<CharacterState>();
            _toolState = GetComponentInParent<ToolState>();
            _toolData = GetComponentInParent<ToolControllerSharedData>();
            
        }

        private float _timeLastToolSelected;
        private void Update()
        {

            ToolState.Inputs.AvailableNumberOfTools = ToolData.AvailableTools;
            int currentIndex = ToolState.Inputs.CurrentToolIndex;
            if (currentIndex != ToolData.CurrentToolIndex)
            {
                ToolData.CurrentToolIndex = currentIndex;
                ToolData.UpdateTool();
            }

            if (Time.realtimeSinceStartup - _timeLastToolSelected > toolSelectionResetTime)
            {
                var toolSelectionInput = ToolState.Inputs.SelectTool;
                if (toolSelectionInput > 0)
                {
                    _timeLastToolSelected = Time.realtimeSinceStartup;
                    ToolData.NextTool();
                }
                else if (toolSelectionInput < 0)
                {
                    _timeLastToolSelected = Time.realtimeSinceStartup;
                    ToolData.PrevTool();
                }
                else
                {
                    return;
                }
            }
        }
    }
}