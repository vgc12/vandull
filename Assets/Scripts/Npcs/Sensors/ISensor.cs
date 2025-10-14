using UnityEngine;

namespace Npcs.Sensors
{
    public interface ISensor
    {
        public bool CanSeeTarget { get; }
        public Transform Target { get; }
    }
}