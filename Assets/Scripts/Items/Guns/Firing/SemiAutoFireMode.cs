using System;
using System.Collections.Generic;
using Items.Guns.Firing;
using UnityEngine;

namespace Items.Guns
{
    public class SemiAutoFireMode : BaseFireMode
    {
        public SemiAutoFireMode(GunConfig config, Transform gunTransform, MonoBehaviour behaviour, List<Action<ShotFiredEvent>> onShotFiredSubscribers = null)
            : base(config, gunTransform, behaviour, onShotFiredSubscribers)
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