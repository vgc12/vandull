using System.Collections.Generic;
using System.Linq;
using Items.Guns.Firing;

namespace Items.Guns
{
    public class FireModeSwitcher : IFireModeSystem
    {
        public IFireSystem CurrentFireSystem { get; private set; }
        public IReadOnlyList<IFireSystem> AvailableFireModes { get; }

        private int _currentIndex = 0;
        public FireModeSwitcher(IReadOnlyList<IFireSystem> availableFireModes)
        {
           
            AvailableFireModes = availableFireModes;
            CurrentFireSystem = availableFireModes[0];
        }

        public void SetCurrentFireMode(FireType fireType)
        {
            if(fireType == FireType.Automatic)
                CurrentFireSystem = AvailableFireModes.First(fm => fm is AutomaticFireMode);
            else if(fireType == FireType.Burst)
                CurrentFireSystem = AvailableFireModes.First(fm => fm is BurstFireMode);
            else if(fireType == FireType.SemiAutomatic)
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