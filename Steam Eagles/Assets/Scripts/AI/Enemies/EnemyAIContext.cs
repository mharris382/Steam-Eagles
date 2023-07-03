using Zenject;

namespace AI.Enemies.Installers
{
    public class EnemyAIContext : AIContext<Enemy>
    {
        [Inject] public Enemy.Config Config { get; }

         
    }
    
    
    
}