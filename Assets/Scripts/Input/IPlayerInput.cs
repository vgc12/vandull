using UnityEngine;
using UnityEngine.Events;

namespace Player.Input
{
    public interface IPlayerInput
    {
        Vector2 Direction { get; }
        event UnityAction QuickReload;
        event UnityAction CheckAmmo;
        event UnityAction<(bool started, bool performed, bool canceled)> Attack;
        event UnityAction<Vector2> Move;
        event UnityAction<Vector2> Look;
        event UnityAction Interact;
        event UnityAction<bool> Crouch;
        event UnityAction<bool> Jump;
        event UnityAction Previous;
        event UnityAction Next;
        event UnityAction<bool> Sprint;
        event UnityAction<float> Lean;
        event UnityAction<bool> Aim;
        event UnityAction<float> SwitchItem;
        event UnityAction Reload;
        event UnityAction SwitchFireMode;
        event UnityAction Restart;
    }
}