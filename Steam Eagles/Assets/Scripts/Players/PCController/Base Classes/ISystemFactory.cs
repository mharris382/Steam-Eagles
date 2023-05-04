using Players.PCController;
using Zenject;

public interface ISystemFactory<out T> : IFactory<PC, T> where T : PCSystem { }