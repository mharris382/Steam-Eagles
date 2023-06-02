using FSM;
using NodeCanvas.Framework;

namespace Characters.Animations.NodeCanvas
{
    public class PlayOneShotAnimation : ActionTask<CharacterAnimation>
    {
        public bool finishImmediately;


        protected override void OnUpdate()
        {
            if (finishImmediately)
            {
                EndAction(true);
            }
            base.OnUpdate();
        }
    }
}