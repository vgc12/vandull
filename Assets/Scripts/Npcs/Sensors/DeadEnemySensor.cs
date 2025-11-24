using System.Linq;

namespace Npcs.Sensors
{
    public class DeadEnemySensor : MultiTargetTypeSensor<Enemy>
    {
        public override bool CanSeeTarget => VisibleTargets.Count > 0 && VisibleTargets.Any(t => t.IsDead);
    }
}