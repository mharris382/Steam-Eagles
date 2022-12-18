namespace Core.SharedVariables
{
    public abstract class SharedReference<T> : SharedVariable<T> where T : class
    {
        public bool HasValue => this.Value != null;
    }
}