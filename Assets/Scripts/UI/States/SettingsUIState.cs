using System;
using System.Collections.Generic;
using DependencyInjection;
using EventBus;
using Player.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace UI.States
{
    public class SettingsUIState : UIBaseState
    {
        private readonly Dictionary<string, InputAction> _actionMap = new();

        // Settings storage
        private readonly Dictionary<string, (string primary, string secondary)> _keyBindings = new();

        private readonly UIStateMachine _stateMachine;
        private Button _applyButton;
        private Button _cancelRebindButton;
        private Button _closeButton;
        private string _currentActionName;
        private int _currentBindingIndex;

        private InputActionAsset _inputActions;

        // UI Elements
        private VisualElement _inputOverlay;
        private Toggle _invertXToggle;
        private Toggle _invertYoggle;

        // Rebinding state
        private InputActionRebindingExtensions.RebindingOperation _rebindOperation;
        private Button _resetButton;
        private Slider _sensitivitySlider;
        private Toggle _toggleAimToggle;
        private Toggle _toggleCrouchToggle;
        private Toggle _toggleSprintToggle;
        private Label _waitingText;

        public SettingsUIState(VisualElement root, UIStateMachine stateMachine, UIStateType stateType) : base(root,
            stateMachine, stateType)
        {
            InitializeInputSystem();
            CacheUIElements();
            SetupEventListeners();
            UpdateAllButtonTexts();
        }

        private void InitializeInputSystem()
        {
            if (!RuntimeResolver.Instance.TryResolve<InputManager>(out var inputManager))
                Logger.LogError("Could not find input manager in scene!");

            _inputActions = inputManager.InputActions.asset;

            // Cache all actions we'll be rebinding
            CacheAction("move-forward", "Player/Move Forward");
            CacheAction("move-backward", "Player/Move Backward");
            CacheAction("move-right", "Player/Move Right");
            CacheAction("move-left", "Player/Move Left");
            CacheAction("lean-left", "Player/Lean Left");
            CacheAction("lean-right", "Player/Lean Right");
            CacheAction("jump", "Player/Jump");
            CacheAction("crouch", "Player/Crouch");
            CacheAction("interact", "Player/Interact");
            CacheAction("fire", "Player/Fire");
            CacheAction("aim", "Player/Aim");
            CacheAction("cycle-forward", "Player/Cycle Forward");
            CacheAction("cycle-backward", "Player/Cycle Backward");
        }

        private void CacheAction(string key, string actionPath)
        {
            var action = _inputActions?.FindAction(actionPath);
            if (action != null) _actionMap[key] = action;
        }

        private void CacheUIElements()
        {
            // Cache overlay elements
            _inputOverlay = RootPageElement.Q<VisualElement>("input-overlay");
            _cancelRebindButton = RootPageElement.Q<Button>("cancel-rebind");
            _waitingText = _inputOverlay?.Q<Label>("waiting-text");

            // Cache bottom buttons
            _applyButton = RootPageElement.Q<Button>("apply-button");
            _resetButton = RootPageElement.Q<Button>("reset-button");
            _closeButton = RootPageElement.Q<Button>("close-button");
            _sensitivitySlider = RootPageElement.Q<Slider>("sensitivity slider");
            _invertYoggle = RootPageElement.Q<Toggle>("invert-y toggle");
            _invertXToggle = RootPageElement.Q<Toggle>("invert-x toggle");
            _toggleCrouchToggle = RootPageElement.Q<Toggle>("toggle-crouch toggle");
            _toggleSprintToggle = RootPageElement.Q<Toggle>("toggle-sprint toggle");
            _toggleAimToggle = RootPageElement.Q<Toggle>("toggle-aim toggle");

            // Hide overlay initially
            if (_inputOverlay != null) _inputOverlay.style.display = DisplayStyle.None;
        }

        private void SetupEventListeners()
        {
            // Setup all keybind buttons
            SetupBindingButtons("move-forward");
            SetupBindingButtons("move-backward");
            SetupBindingButtons("move-right");
            SetupBindingButtons("move-left");
            SetupBindingButtons("lean-left");
            SetupBindingButtons("lean-right");
            SetupBindingButtons("jump");
            SetupBindingButtons("crouch");
            SetupBindingButtons("interact");
            SetupBindingButtons("fire");
            SetupBindingButtons("aim");
            SetupBindingButtons("cycle-forward");
            SetupBindingButtons("cycle-backward");

            // Setup control buttons
            _cancelRebindButton?.RegisterCallback<ClickEvent>(evt => CancelRebind());
            _applyButton?.RegisterCallback<ClickEvent>(evt => ApplySettings());
            _resetButton?.RegisterCallback<ClickEvent>(evt => ResetToDefaults());
            _closeButton?.RegisterCallback<ClickEvent>(evt => UIStateMachine.SettingsBackButtonClicked());
        }

        private void SetupBindingButtons(string actionName)
        {
            var container = RootPageElement.Q<VisualElement>(actionName);
            if (container == null) return;

            var buttonsContainer = container.Q<VisualElement>("buttons");
            if (buttonsContainer == null) return;

            var primaryButton = buttonsContainer.Q<Button>("primary");
            var secondaryButton = buttonsContainer.Q<Button>("secondary");

            primaryButton?.RegisterCallback<ClickEvent>(evt => StartRebind(actionName, 0, primaryButton));
            secondaryButton?.RegisterCallback<ClickEvent>(evt => StartRebind(actionName, 1, secondaryButton));
        }

        private void StartRebind(string actionName, int bindingIndex, Button button)
        {
            if (!_actionMap.ContainsKey(actionName)) return;

            var action = _actionMap[actionName];

            // Disable the action while rebinding
            action.Disable();

            // Show overlay
            if (_inputOverlay != null)
            {
                _inputOverlay.style.display = DisplayStyle.Flex;
                _waitingText.text = $"Press a key for {actionName.Replace("-", " ")}...";
            }

            _currentActionName = actionName;
            _currentBindingIndex = bindingIndex;

            // Start the rebinding operation
            _rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation => OnRebindComplete(button))
                .OnCancel(operation => OnRebindCancelled())
                .Start();
        }

        private void OnRebindComplete(Button button)
        {
            // Get the new binding
            var action = _actionMap[_currentActionName];
            var bindingPath = action.bindings[_currentBindingIndex].effectivePath;

            // Update button text
            button.text = GetBindingDisplayString(bindingPath);

            // Store the binding
            if (!_keyBindings.ContainsKey(_currentActionName)) _keyBindings[_currentActionName] = (null, null);

            var current = _keyBindings[_currentActionName];
            if (_currentBindingIndex == 0)
                _keyBindings[_currentActionName] = (bindingPath, current.secondary);
            else
                _keyBindings[_currentActionName] = (current.primary, bindingPath);

            CleanupRebind();
        }

        private void OnRebindCancelled()
        {
            CleanupRebind();
        }

        private void CancelRebind()
        {
            _rebindOperation?.Cancel();
        }

        private void CleanupRebind()
        {
            _rebindOperation?.Dispose();
            _rebindOperation = null;

            // Hide overlay
            if (_inputOverlay != null) _inputOverlay.style.display = DisplayStyle.None;

            // Re-enable the action
            if (_actionMap.ContainsKey(_currentActionName)) _actionMap[_currentActionName].Enable();
        }

        private void UpdateAllButtonTexts()
        {
            foreach (var kvp in _actionMap) UpdateButtonText(kvp.Key);
        }

        private void UpdateButtonText(string actionName)
        {
            var container = RootPageElement.Q<VisualElement>(actionName);
            if (container == null) return;

            var buttonsContainer = container.Q<VisualElement>("buttons");
            if (buttonsContainer == null) return;

            var primaryButton = buttonsContainer.Q<Button>("primary");
            var secondaryButton = buttonsContainer.Q<Button>("secondary");

            var action = _actionMap[actionName];
            if (action.bindings.Count <= 0) return;
            if (primaryButton != null)
                primaryButton.text = GetBindingDisplayString(action.bindings[0].effectivePath);

            if (action.bindings.Count > 1 && secondaryButton != null)
                secondaryButton.text = GetBindingDisplayString(action.bindings[1].effectivePath);
        }

        private string GetBindingDisplayString(string bindingPath)
        {
            // Clean up the binding path for display
            if (string.IsNullOrEmpty(bindingPath)) return "None";

            var parts = bindingPath.Split('/');
            if (parts.Length <= 0) return bindingPath;

            var key = parts[^1];

            // Format special keys
            key = key.Replace("upArrow", "↑");
            key = key.Replace("downArrow", "↓");
            key = key.Replace("leftArrow", "←");
            key = key.Replace("rightArrow", "→");
            key = key.Replace("leftShift", "L-Shift");
            key = key.Replace("rightShift", "R-Shift");
            key = key.Replace("leftCtrl", "L-Ctrl");
            key = key.Replace("rightCtrl", "R-Ctrl");
            key = key.Replace("space", "Space");

            return key.ToUpper();
        }


        private void ApplySettings()
        {
            EventBus<ControlSettingsChangedEvent>.Raise(new ControlSettingsChangedEvent
            {
                MouseSensitivity = _sensitivitySlider.value,
                InvertX = _invertXToggle.value,
                InvertY = _invertYoggle.value,
                ToggleCrouch = _toggleCrouchToggle.value,
                ToggleSprint = _toggleSprintToggle.value,
                ToggleAim = _toggleAimToggle.value
            });
        }

        private void ResetToDefaults()
        {
            foreach (var action in _actionMap.Values) action.RemoveAllBindingOverrides();

            _keyBindings.Clear();

            UpdateAllButtonTexts();
            Debug.Log("Reset to defaults!");
        }


        public override void Exit()
        {
            base.Exit();

            // Cleanup any active rebinding
            if (_rebindOperation == null) return;
            _rebindOperation.Dispose();
            _rebindOperation = null;
        }


        public struct ControlSettingsChangedEvent : IEvent
        {
            public float MouseSensitivity { get; init; }
            public bool InvertY { get; init; }
            public bool InvertX { get; init; }
            public bool ToggleCrouch { get; init; }
            public bool ToggleSprint { get; init; }
            public bool ToggleAim { get; init; }
        }

        [Serializable]
        private class SerializableBindings
        {
            public Dictionary<string, (string primary, string secondary)> Bindings;
        }
    }
}