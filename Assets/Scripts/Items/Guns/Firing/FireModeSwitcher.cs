using System.Collections.Generic;
using System.Linq;

namespace Items.Guns.Firing
{
    public class FireModeSwitcher : IFireModeSystem
    {
        private int _currentIndex;

        public FireModeSwitcher(IReadOnlyList<IFireSystem> availableFireModes)
        {
            AvailableFireModes = availableFireModes;
            CurrentFireSystem = availableFireModes[0];
        }

        public IFireSystem CurrentFireSystem { get; private set; }
        public IReadOnlyList<IFireSystem> AvailableFireModes { get; }

        public void SetCurrentFireMode(FireType fireType)
        {
            if (fireType == FireType.Automatic)
                CurrentFireSystem = AvailableFireModes.First(fm => fm is AutomaticFireMode);
            else if (fireType == FireType.Burst)
                CurrentFireSystem = AvailableFireModes.First(fm => fm is BurstFireMode);
            else if (fireType == FireType.SemiAutomatic)
                CurrentFireSystem = AvailableFireModes.First(fm => fm is SemiAutoFireMode);
            else
                CurrentFireSystem = AvailableFireModes[0];
        }

        public void CycleFireMode()
        {
            _currentIndex++;
            CurrentFireSystem = AvailableFireModes[_currentIndex % AvailableFireModes.Count];
        }


        public void Update()
        {
            CurrentFireSystem.Update();
        }
    }
}