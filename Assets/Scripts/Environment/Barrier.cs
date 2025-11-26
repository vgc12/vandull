using EventBus;
using Levels.Strategies;
using Player;
using UnityEngine;

namespace Environment
{
    public sealed class Barrier : MonoBehaviour
    {
        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<PlayerStateMachine>())
                EventBus<PlayerKilledEvent>.Raise(new PlayerKilledEvent(other.gameObject, other.transform.position,
                    "Fell Into The Void"));
        }
    }
}