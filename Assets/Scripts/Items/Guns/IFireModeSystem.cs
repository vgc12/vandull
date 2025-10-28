using System.Collections.Generic;
using Items.Guns.Firing;

namespace Items.Guns
{
    public interface IFireModeSystem : IItemSystem
    {
        IFireSystem CurrentFireSystem { get; }
        IReadOnlyList<IFireSystem> AvailableFireModes { get; }

        void SetCurrentFireMode(FireType fireType);

        void CycleFireMode();
    }
}