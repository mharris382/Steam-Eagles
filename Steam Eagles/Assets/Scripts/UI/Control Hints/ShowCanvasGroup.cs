using NodeCanvas.Framework;
using NodeCanvas.StateMachines;
using ParadoxNotion.Design;
using UnityEngine;
using CanvasGroup = UnityEngine.CanvasGroup;

namespace UI.Control_Hints
{
    public abstract class CanvasGroupAction : ActionTask<GameObject>
    {
        protected override void OnExecute()
        {
            if (agent == null)
            {
                EndAction(false);
                return;
            }
            var cg = agent.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                EndAction(false);
                return;
            }
            ExecuteOn(cg);
            EndAction(true);
        }

        protected abstract void ExecuteOn(CanvasGroup cg);
    }
    
    
    [Category("UI/Canvas Group")]
    public class ShowCanvasGroup : CanvasGroupAction
    {
        protected override string info => $"Show {agentInfo}";

        protected override void ExecuteOn(CanvasGroup cg)
        {
            cg.alpha = 1;
        }
    }

    [Category("UI/Canvas Group")]
    public class HideCanvasGroup : CanvasGroupAction
    {
        protected override string info => $"Hide {agentInfo}";
        protected override void ExecuteOn(CanvasGroup cg)
        {
            cg.alpha = 0;
        }
    }
}