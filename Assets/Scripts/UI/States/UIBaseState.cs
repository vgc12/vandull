using DependencyInjection;
using EventBus;
using StateMachine;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using ILogger = General.Logging.ILogger;

namespace UI.States
{
    public enum UIStateType
    {
        InGame,
        LevelSelect,
        MainMenu,
        MainMenuSettings,
        InGameSettings,
        MissionFailed,
        MissionSuccess,
        Paused,
        Quit,
        Loading
    }

    public abstract class UIBaseState : BaseState
    {
        protected readonly ILogger Logger;
        protected readonly VisualElement RootPageElement;
        protected readonly UIStateType StateType;
        protected readonly UIStateMachine UIStateMachine;

        protected UIBaseState(VisualElement rootElement, UIStateMachine uiStateMachine, UIStateType stateType)
        {
            RootPageElement = rootElement;
            StateType = stateType;
            UIStateMachine = uiStateMachine;
            RuntimeResolver.Instance.TryResolve(out Logger);
        }

        public override void Enter()
        {
            RootPageElement.style.display = DisplayStyle.Flex;
            ChangeMouseState();
            EventBus<UIStateSwitchedEvent>.Raise(new UIStateSwitchedEvent(StateType));
        }


        public override void Exit()
        {
            RootPageElement.style.display = DisplayStyle.None;
            UIStateMachine.ResetCommand();
        }

        /// <summary>
        ///     Called on enter, used to change whether the mouse is locked or not
        /// </summary>
        protected virtual void ChangeMouseState()
        {
            UnlockCursorAndShowMouse();
        }

        protected static void LockCursorAndHideMouse()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        protected static void UnlockCursorAndShowMouse()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public struct UIStateSwitchedEvent : IEvent
    {
        public readonly UIStateType NewState;

        public UIStateSwitchedEvent(UIStateType newState)
        {
            NewState = newState;
        }
    }
}