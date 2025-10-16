using General;
using Levels.Strategies;

namespace Levels.Factories
{
    public abstract class KillEnemiesStrategyFactory : IFactory<IMissionStrategy>
    {
        public IMissionStrategy Create()
        {
            return new KillAllEnemiesStrategy();
        }
    }
}