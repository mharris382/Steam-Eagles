namespace CoreLib
{
    public interface ISlowTickable
    {
        void SlowTick(float deltaTime);
    }
    
    
    public interface IExtraSlowTickable
    {
        void ExtraSlowTick(float deltaTime);
    }
}