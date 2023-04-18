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
    }
}