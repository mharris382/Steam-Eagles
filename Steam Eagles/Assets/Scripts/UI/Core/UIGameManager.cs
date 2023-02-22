using System;
using CoreLib;
using UnityEngine;
using FSM;
namespace UI.Core
{
    public class UIGameManager : Singleton<UIGameManager>
    {
        private FSM.StateMachine _rootStateMachine;


        private void Awake()
        {
            _rootStateMachine = new FSM.StateMachine();
        }
    }
}