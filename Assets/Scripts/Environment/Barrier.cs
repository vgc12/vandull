using EventBus;
using Levels;
using Levels.Strategies;
using Player;
using UnityEngine;

namespace Environment
{
    public class Barrier : MonoBehaviour
    {
        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<PlayerStateMachine>())
                EventBus<LevelEvent>.Raise(new LevelEvent(LevelEventType.LevelLost, "Fell Into Oblivion"));
        }
    }
}