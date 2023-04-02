[System.Obsolete("This is super unnecessary unless we were using a DI framework")]
public interface IPlayerDependencyResolver<out T>
{
    public T GetDependency(int playerNumber);
}