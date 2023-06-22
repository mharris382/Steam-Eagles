using Tools.BuildTool;
using UnityEngine;

namespace Tools.UI
{
    public class HUDToolTester : HUDToolControllerBase
    {
        public override void OnFullyInitialized()
        {
                Debug.Log("Tool HUD Tester fully initialized");
        }

        public override void HideToolHUD()
        {
            Debug.Log("Tool HUD Tester Hide tool HUD");
        }

        public override void ShowToolHUD(ToolControllerBase controllerBase)
        { 
            if(controllerBase !=null)
                Debug.Log($"Tool HUD Tester show {controllerBase.name}");
            else
            {
                Debug.LogError("`controllerBase` is null");
            }
        }
    }
}