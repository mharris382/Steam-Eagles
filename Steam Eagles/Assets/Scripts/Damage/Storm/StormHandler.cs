namespace Damage
{
    public class StormHandler
    {
        
    }

    public abstract class StormSystem
    {
        public StormSystem(Storm storm)
        {
            this.Storm = storm;
        }

        public Storm Storm { get; }
    }
}