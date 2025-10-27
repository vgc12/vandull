using System;
using System.Collections.Generic;

namespace Items.Guns.Firing
{
    public class SemiAutoFireMode : BaseFireMode
    {
        public SemiAutoFireMode(Gun gun, List<Action<ShotFiredEvent>> onShotFiredSubscribers = null)
            : base(gun, onShotFiredSubscribers)
        {
        }


        public override void ExecuteFireCommand(FireCommand command)
        {
            switch (command)
            {
                case FireCommand.SingleShot:
                    Fire();
                    break;
                case FireCommand.StartAutomaticFire:
                    break;
                case FireCommand.StopAutomaticFire:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), command, null);
            }
        }

        public override void Fire()
        {
            PerformShot();
        }


        public override void StopFire()
        {
            // Noop
        }
    }
}