using System;
using EventBus;
using UnityEngine;

namespace Player.States
{
    public readonly struct PlayerMovementEnteredEvent : IEvent
    {
        public readonly Rigidbody Rigidbody;
        public readonly Type StateType;

        public PlayerMovementEnteredEvent(Rigidbody rb, Type stateType)
        {
            Rigidbody = rb;
            StateType = stateType;
        }
    }

    public readonly struct PlayerMovementExitedEvent : IEvent
    {
        public readonly Rigidbody Rigidbody;
        public readonly Type StateType;

        public PlayerMovementExitedEvent(Rigidbody rb, Type stateType)
        {
            Rigidbody = rb;
            StateType = stateType;
        }
    }
}