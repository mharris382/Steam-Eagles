namespace StateMachine
{
    public interface IState<in T>
    {
        public void Tick(T actor);
        public void FixedTick(T actor);
        public void OnEnter(T actor);
        public void OnExit(T actor);
    }

    public class NullState<T> : IState<T>
    {
        public void Tick(T actor)
        {
        }

        public void FixedTick(T actor)
        {
        }

        public void OnEnter(T actor)
        {
        }

        public void OnExit(T actor)
        {
        }
    }
}