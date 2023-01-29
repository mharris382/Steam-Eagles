public interface IPlayerDependencyResolver<out T>
{
    public T GetDependency(int playerNumber);
}