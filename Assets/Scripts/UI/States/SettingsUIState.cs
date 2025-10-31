using System;
using System.Collections.Generic;
using Audio;
using DependencyInjection;
using EventBus;
using Player.Input;
using Unity.VisualScripting;
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

        private Button _applyButton;
        private Button _cancelRebindButton;
        private Button _closeButton;
        private string _currentActionName;
        private int _currentBindingIndex;

        private InputActionAsset _inputActions;

        // UI Elements
        private VisualElement _inputOverlay;
        private Toggle _invertXToggle;
        private Toggle _invertYToggle;

        // Rebinding state
        private InputActionRebindingExtensions.RebindingOperation _rebindOperation;
        private Button _resetButton;
        private Slider _sensitivitySlider;
        private Toggle _toggleAimToggle;
        private Toggle _toggleCrouchToggle;
        private Toggle _toggleSprintToggle;
        private Label _waitingText;
        
        //Audio Sliders
        private SliderInt _mainVolumeSlider;
        private SliderInt _musicVolumeSlider;
        private SliderInt _sfxVolumeSlider;
        private SliderInt _dialogueVolumeSlider;
        private SliderInt _ambientVolumeSlider;
        
        
        

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
            
            // Cache composite actions
            CacheAction("move", "Player/Move");
            CacheAction("lean", "Player/Lean");
            CacheAction("switch-item", "Player/SwitchItem");
    
            // Cache simple button actions
            CacheAction("jump", "Player/Jump");
            CacheAction("crouch", "Player/Crouch");
            CacheAction("interact", "Player/Interact");
            CacheAction("attack", "Player/Attack");
            CacheAction("aim", "Player/Aim");
            CacheAction("sprint", "Player/Sprint");
            CacheAction("reload", "Player/Reload");
        
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
            _waitingText = _inputOverlay?.Q<Label>("waiting-label");

            // Cache bottom buttons
            _applyButton = RootPageElement.Q<Button>("apply-button");
            _resetButton = RootPageElement.Q<Button>("reset-button");
            _closeButton = RootPageElement.Q<Button>("close-button");
            _sensitivitySlider = RootPageElement.Query<VisualElement>("sensitivity").Children<Slider>().First();
            _invertYToggle = RootPageElement.Query<VisualElement>("invert-y").Children<Toggle>().First();
            _invertXToggle = RootPageElement.Query<VisualElement>("invert-x").Children<Toggle>().First();

            _toggleCrouchToggle = RootPageElement.Query<VisualElement>("toggle-crouch").Children<Toggle>().First();
            _toggleSprintToggle = RootPageElement.Query<VisualElement>("toggle-sprint").Children<Toggle>().First();
            _toggleAimToggle = RootPageElement.Query<VisualElement>("toggle-aim").Children<Toggle>().First();

            _mainVolumeSlider = RootPageElement.Query<VisualElement>("main").Children<SliderInt>().First();
            _musicVolumeSlider = RootPageElement.Query<VisualElement>("music").Children<SliderInt>().First();
            _sfxVolumeSlider = RootPageElement.Query<VisualElement>("sound-effects").Children<SliderInt>().First();
            _dialogueVolumeSlider = RootPageElement.Query<VisualElement>("dialogue").Children<SliderInt>().First();
            _ambientVolumeSlider = RootPageElement.Query<VisualElement>("ambient").Children<SliderInt>().First();

            RetrieveSettings();
            
            _currentControlSettings = new ControlSettingsChangedEvent(this);
            _currentAudioSettings = new AudioSettingsChangedEvent(this);
            
            // Hide overlay initially
            if (_inputOverlay != null) _inputOverlay.style.display = DisplayStyle.None;
        }

        private void RetrieveSettings()
        {
            // get settings from file
        }


        private void SetupEventListeners()
        {
            // Setup composite movement bindings (WASD composite has multiple bindings per direction)
            SetupCompositeBindings("move");
    
            // Setup composite lean bindings (1D Axis)
            SetupCompositeBindings("lean");
    
            // Setup individual action bindings
            SetupBindingButtons("jump");
            SetupBindingButtons("crouch");
            SetupBindingButtons("interact");
            SetupBindingButtons("attack");
            SetupBindingButtons("aim");
            SetupBindingButtons("sprint");
            SetupBindingButtons("reload");

            // Setup control buttons
            _cancelRebindButton.clicked += CancelRebind;
            _applyButton.clicked += ApplySettings;
            _resetButton.clicked += ResetToDefaults;
            _closeButton.clicked += UIStateMachine.SettingsBackButtonClicked;
        }
private void SetupCompositeBindings(string actionName)
{
    if (!_actionMap.TryGetValue(actionName, out var action)) return;
    
    if (actionName == "move")
    {
        // Move has a Dpad composite with up/down/left/right parts
        // Each part can have MULTIPLE bindings (W and UpArrow for up, etc.)
        SetupMultipleCompositePartBindings(actionName, "up", "move-forward");
        SetupMultipleCompositePartBindings(actionName, "down", "move-backward");
        SetupMultipleCompositePartBindings(actionName, "left", "move-left");
        SetupMultipleCompositePartBindings(actionName, "right", "move-right");
    }
    else if (actionName == "lean")
    {
        // Lean has a 1DAxis composite with negative/positive parts
        SetupMultipleCompositePartBindings(actionName, "negative", "lean-left");
        SetupMultipleCompositePartBindings(actionName, "positive", "lean-right");
    }
}

private void SetupMultipleCompositePartBindings(string actionName, string partName, string uiElementName)
{
    var container = RootPageElement.Q<VisualElement>(uiElementName);
    if (container == null) return;

    var buttonsContainer = container.Q<VisualElement>("buttons");
    if (buttonsContainer == null) return;

    var primaryButton = buttonsContainer.Q<Button>("primary");
    var secondaryButton = buttonsContainer.Q<Button>("secondary");
    
    if (!_actionMap.TryGetValue(actionName, out var action)) return;
    
    // Find ALL binding indices for this composite part
    var partIndices = FindCompositePartIndices(action, partName);
    if (partIndices.Count == 0) return;

    // Primary button gets the first binding
    if (partIndices.Count > 0 && primaryButton != null)
    {
        primaryButton.clicked += () => StartCompositeRebind(actionName, partIndices[0], primaryButton);
    }
    
    // Secondary button gets the second binding (if it exists)
    if (partIndices.Count > 1 && secondaryButton != null)
    {
        secondaryButton.clicked += () => StartCompositeRebind(actionName, partIndices[1], secondaryButton);
    }
}

private List<int> FindCompositePartIndices(InputAction action, string partName)
{
    var indices = new List<int>();
    
    // Find all composite bindings first
    for (int i = 0; i < action.bindings.Count; i++)
    {
        if (action.bindings[i].isComposite)
        {
            // Search for all parts with the matching name after this composite
            for (int j = i + 1; j < action.bindings.Count; j++)
            {
                var binding = action.bindings[j];
                
                // Stop if we hit another composite or a non-part binding
                if (binding.isComposite || !binding.isPartOfComposite)
                    break;
                
                if (binding.name.Equals(partName, StringComparison.OrdinalIgnoreCase))
                {
                    indices.Add(j);
                }
            }
        }
    }
    
    return indices;
}

private void StartCompositeRebind(string actionName, int bindingIndex, Button button)
{
    if (!_actionMap.TryGetValue(actionName, out var action)) return;

    // Disable the action while rebinding
    action.Disable();

    // Show overlay
    if (_inputOverlay != null)
    {
        _inputOverlay.style.display = DisplayStyle.Flex;
        var bindingName = action.bindings[bindingIndex].name;
        _waitingText.text = $"Press a key for {bindingName}...";
    }

    _currentActionName = actionName;
    _currentBindingIndex = bindingIndex;

    // Start the rebinding operation for the specific composite part
    _rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
        .WithControlsExcluding("Mouse")
        .WithCancelingThrough("<Keyboard>/escape")
        .OnMatchWaitForAnother(0.1f)
        .OnComplete(operation => OnRebindComplete(button))
        .OnCancel(operation => OnRebindCancelled())
        .Start();
}

        private void SetupBindingButtons(string actionName)
        {
            var container = RootPageElement.Q<VisualElement>(actionName);
            if (container == null) return;

            var buttonsContainer = container.Q<VisualElement>("buttons");
            if (buttonsContainer == null) return;

            var primaryButton = buttonsContainer.Q<Button>("primary");
            var secondaryButton = buttonsContainer.Q<Button>("secondary");

            primaryButton.clicked += ()=> StartRebind(actionName, 0, primaryButton);
            secondaryButton.clicked += ()=> StartRebind(actionName, 1, secondaryButton);
        }

        private void StartRebind(string actionName, int bindingIndex, Button button)
        {
            if (!_actionMap.TryGetValue(actionName, out var action)) return;

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
        private void UpdateCompositePartButtons(InputAction action, string partName, string uiElementName)
        {
            var container = RootPageElement.Q<VisualElement>(uiElementName);
            if (container == null) return;

            var buttonsContainer = container.Q<VisualElement>("buttons");
            if (buttonsContainer == null) return;

            var primaryButton = buttonsContainer.Q<Button>("primary");
            var secondaryButton = buttonsContainer.Q<Button>("secondary");
    
            var partIndices = FindCompositePartIndices(action, partName);
    
            // Update primary button with first binding
            if (partIndices.Count > 0 && primaryButton != null)
            {
                primaryButton.text = GetBindingDisplayString(action.bindings[partIndices[0]].effectivePath);
            }
    
            // Update secondary button with second binding
            if (partIndices.Count > 1 && secondaryButton != null)
            {
                secondaryButton.text = GetBindingDisplayString(action.bindings[partIndices[1]].effectivePath);
            }
            else if (secondaryButton != null)
            {
                secondaryButton.text = "None";
            }
        }
        private void UpdateCompositeButtonTexts(string actionName)
        {
            if (!_actionMap.TryGetValue(actionName, out var action)) return;
    
            if (actionName == "move")
            {
                UpdateCompositePartButtons(action, "up", "move-forward");
                UpdateCompositePartButtons(action, "down", "move-backward");
                UpdateCompositePartButtons(action, "left", "move-left");
                UpdateCompositePartButtons(action, "right", "move-right");
            }
            else if (actionName == "lean")
            {
                UpdateCompositePartButtons(action, "negative", "lean-left");
                UpdateCompositePartButtons(action, "positive", "lean-right");
            }
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
            if (_actionMap.TryGetValue(_currentActionName, out var value)) value.Enable();
            
        }

        private void UpdateAllButtonTexts()
        {
            foreach (var kvp in _actionMap)
            {
                if (kvp.Key == "move" || kvp.Key == "lean")
                {
                    UpdateCompositeButtonTexts(kvp.Key);
                }
                else
                {
                    UpdateButtonText(kvp.Key);
                }
            }
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

        private ControlSettingsChangedEvent _currentControlSettings;
        private AudioSettingsChangedEvent _currentAudioSettings;

        private void ApplySettings()
        {
            EventBus<ControlSettingsChangedEvent>.Raise(_currentControlSettings);
            EventBus<AudioSettingsChangedEvent>.Raise(_currentAudioSettings);
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
            public float MouseSensitivity { get; private set; }
            public bool InvertY  { get; private set; }
            public bool InvertX  { get; private set; }
            public bool ToggleCrouch { get; private set; }
            public bool ToggleSprint  { get; private set; }
            public bool ToggleAim  { get; private set; }
            
            private readonly SettingsUIState _settingsUIState;

            public ControlSettingsChangedEvent(SettingsUIState settingsUIState) : this()
            {
                _settingsUIState = settingsUIState;
                UpdateSettings();
            }

            public void UpdateSettings()
            {
                MouseSensitivity = _settingsUIState._sensitivitySlider.value;
                InvertY = _settingsUIState._invertYToggle.value;
                InvertX = _settingsUIState._invertXToggle.value;
                ToggleCrouch = _settingsUIState._toggleCrouchToggle.value;
                ToggleSprint = _settingsUIState._toggleSprintToggle.value;
                ToggleAim = _settingsUIState._toggleAimToggle.value;
            }
        }
        public struct AudioSettingsChangedEvent : IEvent
        {
            public float MainVolume { get; private set; }
            public float MusicVolume { get; private set; }
        
            public float SoundEffectsVolume { get; private set; }
        
            public float DialogueVolume { get; private set; }
        
            public float Ambient { get; private set; }
            
            private readonly SettingsUIState _settingsUIState;
            
            public AudioSettingsChangedEvent(SettingsUIState settingsUIState) : this()
            {
               _settingsUIState = settingsUIState;
               UpdateSettings();
            }

            private void UpdateSettings()
            {
                MainVolume = _settingsUIState._mainVolumeSlider.value;
                MusicVolume = _settingsUIState._musicVolumeSlider.value;
                SoundEffectsVolume = _settingsUIState._sfxVolumeSlider.value;
                DialogueVolume = _settingsUIState._dialogueVolumeSlider.value;
                Ambient = _settingsUIState._ambientVolumeSlider.value;
            }
        }
        
        
        
        [Serializable]
        private class SerializableBindings
        {
            public Dictionary<string, (string primary, string secondary)> Bindings;
        }
    }
}