using System;
using System.Collections.Generic;
using DependencyInjection;
using EventBus;
using Player.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UIElements;

namespace UI.States
{
    [Serializable]
    public class KeyIconMapping
    {
        public string inputPath;
        public string iconName;

        public KeyIconMapping(string inputPath, string iconName)
        {
            this.inputPath = inputPath;
            this.iconName = iconName;
        }
    }

    public class SettingsUIState : UIBaseState
    {
        private readonly Dictionary<string, InputActionReference> _actionMap = new();

        // Current device type detection
        private string _currentDeviceFolder = "Xbox Series";
        private readonly KeyIconMappingConfig _keyIconMappingConfig = new();

        // Configuration for composite actions
        private readonly Dictionary<string, Dictionary<string, string>> _compositeConfig = new()
        {
            ["move"] = new Dictionary<string, string>
            {
                ["up"] = "move-forward",
                ["down"] = "move-backward",
                ["left"] = "move-left",
                ["right"] = "move-right"
            },
            ["lean"] = new Dictionary<string, string>
            {
                ["negative"] = "lean-left",
                ["positive"] = "lean-right"
            },
            ["switch-item"] = new Dictionary<string, string>
            {
                ["positive"] = "cycle-forward",
                ["negative"] = "cycle-backward"
            }
        };

        // Settings storage
        // <Action name , binding path>
        private readonly Dictionary<string, string> _keyBindings = new();
        private SliderInt _ambientVolumeSlider;

        private Button _applyButton;
        private Button _cancelRebindButton;
        private Button _closeButton;
        private string _currentActionName;
        private AudioSettingsChangedEvent _currentAudioSettings;
        private int _currentBindingIndex;


        public enum ControlScheme
        {
            KeyboardMouse,
            Gamepad
        }

        private ControlScheme _currentControlScheme = ControlScheme.KeyboardMouse;
        private Tab _keyboardMouseTab;
        private Tab _gamepadTab;


        private ControlSettingsChangedEvent _currentControlSettings;
        private SliderInt _dialogueVolumeSlider;

        private InputManager _inputActions;

        // UI Elements
        private VisualElement _inputOverlay;
        private Toggle _invertXToggle;
        private Toggle _invertYToggle;

        //Audio Sliders
        private SliderInt _mainVolumeSlider;
        private SliderInt _musicVolumeSlider;

        // Rebinding state
        private InputActionRebindingExtensions.RebindingOperation _rebindOperation;
        private Button _resetButton;
        private Slider _sensitivitySlider;
        private SliderInt _sfxVolumeSlider;
        private Toggle _toggleAimToggle;
        private Toggle _toggleCrouchToggle;
        private Toggle _toggleSprintToggle;
        private SliderInt _uiVolumeSlider;
        private Label _waitingText;


        public SettingsUIState(VisualElement root, UIStateMachine stateMachine, UIStateType stateType) : base(root,
            stateMachine, stateType)
        {
            CacheUIElements();
            InitializeInputSystem();
            SetupEventListeners();
            UpdateAllButtonTexts();
        }

        private void SetupDeviceChangeCallbacks()
        {
            // Subscribe to device change events
            InputSystem.onActionChange += OnActionChange;
        }

        private void OnActionChange(object obj, InputActionChange change)
        {
            // Detect when actions are being used (which indicates active device)
            if (change == InputActionChange.ActionPerformed)
                if (obj is InputAction action)
                {
                    var device = action.activeControl?.device;
                    if (device != null) UpdateCurrentDeviceFromControl(device);
                }
        }

        private void UpdateCurrentDeviceFromControl(InputDevice device)
        {
            var oldScheme = _currentControlScheme;

            if (device is Gamepad)
            {
                _currentControlScheme = ControlScheme.Gamepad;

                // Detect specific gamepad type
                if (device is DualShockGamepad)
                    _currentDeviceFolder = "PS5";
                else if (device is XInputController)
                    _currentDeviceFolder = "Xbox Series";
                else if (device is SwitchProControllerHID)
                    _currentDeviceFolder = "Switch";
                else
                    _currentDeviceFolder = "Xbox Series"; // Default gamepad
            }

            // Update UI if scheme changed
            if (oldScheme != _currentControlScheme) OnControlSchemeChanged();
        }

        private void OnControlSchemeChanged()
        {
            UpdateAllButtonTexts();
        }

        private void InitializeInputSystem()
        {
            if (!RuntimeResolver.Instance.TryResolve(out _inputActions))
                Logger.LogError("Could not find input manager in scene!");


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
            var action = InputActionReference.Create(_inputActions.InputActions.asset.FindAction(actionPath));
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

            _mainVolumeSlider = RootPageElement.Query<VisualElement>("main").Children<SliderInt>("slider").First();
            _musicVolumeSlider = RootPageElement.Query<VisualElement>("music").Children<SliderInt>("slider").First();
            _sfxVolumeSlider = RootPageElement.Query<VisualElement>("sound-effects").Children<SliderInt>("slider")
                .First();
            _dialogueVolumeSlider =
                RootPageElement.Query<VisualElement>("dialogue").Children<SliderInt>("slider").First();
            _ambientVolumeSlider =
                RootPageElement.Query<VisualElement>("ambient").Children<SliderInt>("slider").First();
            _uiVolumeSlider = RootPageElement.Query<VisualElement>("ui").Children<SliderInt>("slider").First();

            _keyboardMouseTab = RootPageElement.Q<Tab>("keyboard-binds-tab");
            _gamepadTab = RootPageElement.Q<Tab>("controller-binds-tab");


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

            SetupCompositeBindings("switch-item");

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

            _keyboardMouseTab.selected += _ => _currentControlScheme = ControlScheme.KeyboardMouse;
            _gamepadTab.selected += _ => _currentControlScheme = ControlScheme.Gamepad;
        }

        private void SetupCompositeBindings(string actionName)
        {
            if (!_actionMap.TryGetValue(actionName, out var reference)) return;
            reference.action.Disable();

            if (!_compositeConfig.TryGetValue(actionName, out var parts)) return;

            foreach (var (partName, uiElementName) in parts)
                SetupCompositePartBindings(actionName, partName, uiElementName);
        }

        private void SetupCompositePartBindings(string actionName, string partName, string uiElementName)
        {
            if (!_actionMap.TryGetValue(actionName, out var action)) return;

            var keyboardButton = GetBindButtonsUnderTab(_keyboardMouseTab, uiElementName);
            UpdateButtonWithIcon(keyboardButton, _keyBindings.GetValueOrDefault(actionName, ""));
            var controllerButton = GetBindButtonsUnderTab(_gamepadTab, uiElementName);
            UpdateButtonWithIcon(controllerButton, _keyBindings.GetValueOrDefault(actionName, ""),
                _currentDeviceFolder);

            var partIndices = FindCompositePartIndices(action, partName);
            if (partIndices.Count == 0) return;

            // Setup click handlers for primary binding (first index)
            if (keyboardButton != null)
                keyboardButton.clicked += () => StartRebind(actionName, partIndices[0], keyboardButton);

            if (controllerButton != null)
                controllerButton.clicked += () => StartRebind(actionName, partIndices[0], controllerButton);
        }

        private List<int> FindCompositePartIndices(InputAction action, string partName)
        {
            var indices = new List<int>();

            for (var i = 0; i < action.bindings.Count; i++)
            {
                if (!action.bindings[i].isComposite) continue;

                // Search for all parts with matching name after this composite
                for (var j = i + 1; j < action.bindings.Count; j++)
                {
                    var binding = action.bindings[j];

                    // Stop if we hit another composite or non-part binding
                    if (binding.isComposite || !binding.isPartOfComposite)
                        break;

                    if (binding.name.Equals(partName, StringComparison.OrdinalIgnoreCase))
                        indices.Add(j);
                }
            }

            return indices;
        }

        private void SetupBindingButtons(string actionName)
        {
            var keyboardButton = GetBindButtonsUnderTab(_keyboardMouseTab, actionName);
            UpdateButtonWithIcon(keyboardButton, _keyBindings.GetValueOrDefault(actionName, ""));
            var controllerButton = GetBindButtonsUnderTab(_gamepadTab, actionName);
            UpdateButtonWithIcon(controllerButton, _keyBindings.GetValueOrDefault(actionName, ""),
                _currentDeviceFolder);

            keyboardButton.clicked += () => StartRebind(actionName, 0, keyboardButton);
            controllerButton.clicked += () => StartRebind(actionName, 0, controllerButton);
        }

        private void StartRebind(string actionName, int bindingIndex, Button button)
        {
            if (!_actionMap.TryGetValue(actionName, out var reference)) return;

            reference.action.Disable();

            if (_inputOverlay != null)
            {
                _inputOverlay.style.display = DisplayStyle.Flex;

                // Get display text - use binding name for composites, action name for simple bindings
                var binding = reference.action.bindings[bindingIndex];
                var displayText = binding.isPartOfComposite
                    ? binding.name
                    : actionName.Replace("-", " ");

                _waitingText.text = $"Press a key for {displayText}...";
            }

            _currentActionName = actionName;
            _currentBindingIndex = bindingIndex;

            // Create rebinding operation with control scheme filtering
            var rebindOp = reference.action.PerformInteractiveRebinding(bindingIndex)
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation => OnRebindComplete(button))
                .OnCancel(operation => OnRebindCancelled());

            // Filter based on current control scheme
            if (_currentControlScheme == ControlScheme.KeyboardMouse)
                // Only allow keyboard and mouse inputs
                rebindOp
                    .WithControlsExcluding("<Gamepad>")
                    .WithControlsExcluding("<XInputController>")
                    .WithControlsExcluding("<DualShockGamepad>")
                    .WithControlsExcluding("<SwitchProController>")
                    .WithCancelingThrough("<Keyboard>/escape");
            else // Gamepad
                // Only allow gamepad inputs
                rebindOp
                    .WithControlsExcluding("<Keyboard>")
                    .WithControlsExcluding("<Mouse>")
                    .WithCancelingThrough("<Gamepad>/buttonEast"); // B/Circle button

            _rebindOperation = rebindOp.Start();
        }


        private void OnRebindComplete(Button button)
        {
            // Get the new binding
            var reference = _actionMap[_currentActionName];
            var bindingPath = reference.action.bindings[_currentBindingIndex].effectivePath;
            Debug.Log($"Rebinding complete {bindingPath}" + _inputActions.InputActions.bindings);

            // Update button with icon or text
            UpdateButtonWithIcon(button, bindingPath);

            // Store the binding
            _keyBindings.TryAdd(_currentActionName, bindingPath);


            _keyBindings[_currentActionName] = bindingPath;

            reference.action.Enable();
            CleanupRebind();
        }

        private void UpdateButtonWithIcon(Button button, string bindingPath, string deviceFolder = "Keyboard & Mouse")
        {
            // Clear existing content
            button.Clear();
            button.text = "";

            if (string.IsNullOrEmpty(bindingPath))
            {
                button.text = "None";
                return;
            }


            // Try to load icon from Resources
            var name = _keyIconMappingConfig.GetIconName(bindingPath);
            if (name != default)
            {
                // Build full path: UI/Icons/Light/{DeviceFolder}/{IconName}
                var iconPath = $"UI/Icons/{deviceFolder}/{name}";

                var texture = Resources.Load<Texture2D>(iconPath);


                if (texture != null)
                {
                    var image = texture;
                    // image.AddToClassList("key-icon");
                    button.style.backgroundImage = new StyleBackground(image);
                    return;
                }

                Debug.LogWarning($"Icon not found at path: {iconPath} for binding: {bindingPath}");
                // Fallback to text if icon not found
                button.text = GetBindingDisplayString(bindingPath);
            }
            else
            {
                // Fallback to text if no icon mapping found
                button.text = GetBindingDisplayString(bindingPath);
            }
        }

        private void UpdateCompositeButtonTexts(string actionName)
        {
            if (!_actionMap.TryGetValue(actionName, out var action)) return;
            if (!_compositeConfig.TryGetValue(actionName, out var parts)) return;

            foreach (var (partName, uiElementName) in parts) UpdateCompositePartButton(action, partName, uiElementName);
        }

        private void UpdateCompositePartButton(InputAction action, string partName, string uiElementName)
        {
            var keyboardButton = GetBindButtonsUnderTab(_keyboardMouseTab, uiElementName);
            UpdateButtonWithIcon(keyboardButton, action.bindings[0].effectivePath);
            var controllerButton = GetBindButtonsUnderTab(_gamepadTab, uiElementName);
            UpdateButtonWithIcon(controllerButton, action.bindings[0].effectivePath, _currentDeviceFolder);

            var partIndices = FindCompositePartIndices(action, partName);

            // Update button with first binding
            if (partIndices.Count > 0 && keyboardButton != null)
                UpdateButtonWithIcon(keyboardButton, action.bindings[partIndices[0]].effectivePath);
            if (partIndices.Count > 0 && controllerButton != null)
                UpdateButtonWithIcon(controllerButton, action.bindings[partIndices[0]].effectivePath);
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
            if (_actionMap.TryGetValue(_currentActionName, out var value)) value.action.Enable();
        }

        private void UpdateAllButtonTexts()
        {
            foreach (var kvp in _actionMap)
                if (kvp.Key == "move" || kvp.Key == "lean" || kvp.Key == "switch-item")
                    UpdateCompositeButtonTexts(kvp.Key);
                else
                    UpdateButtonText(kvp.Key);
        }

        private void UpdateButtonText(string actionName)
        {
            var reference = _actionMap[actionName];
            if (reference.action.bindings.Count <= 0) return;

            var primaryButton = GetBindButtonsUnderTab(_keyboardMouseTab, actionName);

            if (primaryButton != null)
                UpdateButtonWithIcon(primaryButton, reference.action.bindings[0].effectivePath);
        }

        private static Button GetBindButtonsUnderTab(Tab t, string actionName)
        {
            var container = t.Q<VisualElement>(actionName);

            var buttonsContainer = container.Q<VisualElement>("buttons");

            var primaryButton = buttonsContainer.Q<Button>("primary");
            return primaryButton;
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
            _currentControlSettings.UpdateSettings();
            EventBus<ControlSettingsChangedEvent>.Raise(_currentControlSettings);
            _currentAudioSettings.UpdateSettings();
            EventBus<AudioSettingsChangedEvent>.Raise(_currentAudioSettings);
        }

        private void ResetToDefaults()
        {
            foreach (var reference in _actionMap.Values) reference.action.RemoveAllBindingOverrides();

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
            public bool InvertY { get; private set; }
            public bool InvertX { get; private set; }
            public bool ToggleCrouch { get; private set; }
            public bool ToggleSprint { get; private set; }
            public bool ToggleAim { get; private set; }

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

            public float UIVolume { get; private set; }

            private readonly SettingsUIState _settingsUIState;

            public AudioSettingsChangedEvent(SettingsUIState settingsUIState) : this()
            {
                _settingsUIState = settingsUIState;
                UpdateSettings();
            }

            public void UpdateSettings()
            {
                MainVolume = _settingsUIState._mainVolumeSlider.value;
                MusicVolume = _settingsUIState._musicVolumeSlider.value;
                SoundEffectsVolume = _settingsUIState._sfxVolumeSlider.value;
                DialogueVolume = _settingsUIState._dialogueVolumeSlider.value;
                Ambient = _settingsUIState._ambientVolumeSlider.value;
                UIVolume = _settingsUIState._uiVolumeSlider.value;
            }

            public float GetVolumeForCategory(string categoryName)
            {
                return categoryName.ToLower() switch
                {
                    "master" => MainVolume,
                    "music" => MusicVolume,
                    "soundeffects" => SoundEffectsVolume,
                    "dialogue" => DialogueVolume,
                    "ambient" => Ambient,
                    "ui" => UIVolume,
                    _ => 1f
                };
            }
        }


        [Serializable]
        private class SerializableBindings
        {
            public Dictionary<string, string> Bindings;
        }
    }
}