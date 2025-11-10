using UnityEngine;
using UnityEngine.Events;

namespace Player.Input
{
    public interface IUIInput
    {
        event UnityAction InGameCancel;
        event UnityAction UIDisengaged;
        event UnityAction<Vector2> Navigate;
        event UnityAction Submit;
        event UnityAction InMenuCancel;
        event UnityAction<Vector2> Point;
        event UnityAction Click;
        event UnityAction RightClick;
        event UnityAction MiddleClick;
        event UnityAction<Vector2> ScrollWheel;
        event UnityAction<Vector3> TrackedDevicePosition;
        event UnityAction<Quaternion> TrackedDeviceOrientation;
    }
}