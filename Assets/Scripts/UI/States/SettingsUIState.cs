using System;
using System.Collections.Generic;
using System.Linq;
using DependencyInjection;
using EventBus;
using Player.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.UIElements;
using ILogger = General.Logging.ILogger;

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
        public enum ControlScheme
        {
            KeyboardMouse,
            Gamepad
        }

        private const float ScrollCooldownTime = 0.05f;

        private readonly Dictionary<string, InputActionReference> _actionMap = new();
        private readonly VisualElement[] _bottomButtons = new VisualElement[3];

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

        private readonly List<VisualElement> _focusableElements = new();

        private readonly float _inputDelay = 0.1f;

        private readonly KeyIconMappingConfig _keyIconMappingConfig = new();
        private readonly ILogger _logger = RuntimeResolver.Instance.Resolve<ILogger>();
        private SliderInt _ambientVolumeSlider;

        private Button _applyButton;
        private VisualElement _audioContainer;
        private Button _audioTabButton;
        private int _bottomButtonFocusIndex;
        private Button _cancelRebindButton;
        private Button _closeButton;
        private VisualElement _controllerBindsContainer;
        private Button _controllerBindTabButton;
        private VisualElement _controlsContainer;
        private ScrollView _controlsScrollView;

        private Button _controlsTabButton;
        private string _currentActionName;
        private AudioSettingsChangedEvent _currentAudioSettings;
        private int _currentBindingIndex;

        private ControlScheme _currentControlScheme = ControlScheme.KeyboardMouse;


        private ControlSettingsChangedEvent _currentControlSettings;

        // Current device type detection
        private string _currentDeviceFolder = "Xbox Series";
        private int _currentFocusIndex;

        private VisualElement _currentlyFocusedElement;
        private SliderInt _dialogueVolumeSlider;


        private InputManager _inputActions;

        // UI Elements
        private VisualElement _inputOverlay;
        private Toggle _invertXToggle;
        private Toggle _invertYToggle;

        private bool _isScrollViewFocused;
        private VisualElement _keyboardBindsContainer;

        private Button _keyboardBindTabButton;
        private float _lastInputTime;

        //Audio Sliders
        private SliderInt _mainVolumeSlider;
        private SliderInt _musicVolumeSlider;

        // Rebinding state
        private InputActionRebindingExtensions.RebindingOperation _rebindOperation;
        private Button _resetButton;

        private float _scrollCooldown;
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
            SetupDeviceChangeCallbacks();
        }


        private void SetupDeviceChangeCallbacks()
        {
            // Subscribe to device change events
            InputSystem.onActionChange += OnActionChange;
        }

        private void OnActionChange(object obj, InputActionChange change)
        {
            // Detect when actions are being used (which indicates active device)
            if (change != InputActionChange.ActionPerformed) return;
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
            _keyboardBindsContainer = RootPageElement.Q<VisualElement>("keyboard-binds-container");
            _controllerBindsContainer = RootPageElement.Q<VisualElement>("controller-binds-container");
            _keyboardBindTabButton = RootPageElement.Q<Button>("keyboard-tab-button");
            _controllerBindTabButton = RootPageElement.Q<Button>("controller-tab-button");
            _keyboardBindTabButton.clicked += () =>
            {
                _currentControlScheme = ControlScheme.KeyboardMouse;
                _controllerBindsContainer.style.display = DisplayStyle.None;
                _keyboardBindsContainer.style.display = DisplayStyle.Flex;
            };
            _controllerBindTabButton.clicked += () =>
            {
                _currentControlScheme = ControlScheme.Gamepad;
                _keyboardBindsContainer.style.display = DisplayStyle.None;
                _controllerBindsContainer.style.display = DisplayStyle.Flex;
            };

            _controlsTabButton = RootPageElement.Q<Button>("controls-tab-button");
            _audioTabButton = RootPageElement.Q<Button>("audio-tab-button");
            _controlsContainer = RootPageElement.Q<VisualElement>("controls-container");
            _audioContainer = RootPageElement.Q<VisualElement>("audio-container");
            _controlsTabButton.clicked += () =>
            {
                _audioContainer.style.display = DisplayStyle.None;
                _controlsContainer.style.display = DisplayStyle.Flex;
            };
            _audioTabButton.clicked += () =>
            {
                _controlsContainer.style.display = DisplayStyle.None;
                _audioContainer.style.display = DisplayStyle.Flex;
            };
            // Cache overlay elements
            _inputOverlay = RootPageElement.Q<VisualElement>("input-overlay");
            _cancelRebindButton = RootPageElement.Q<Button>("cancel-rebind");
            _waitingText = _inputOverlay?.Q<Label>("waiting-label");


            // Cache bottom buttons
            _applyButton = RootPageElement.Q<Button>("apply-button");
            _resetButton = RootPageElement.Q<Button>("reset-button");
            _closeButton = RootPageElement.Q<Button>("close-button");
            _sensitivitySlider = RootPageElement.Q<Slider>("sensitivity");
            _invertYToggle = RootPageElement.Q<Toggle>("invert-y");
            _invertXToggle = RootPageElement.Q<Toggle>("invert-x");

            _toggleCrouchToggle = RootPageElement.Query<VisualElement>("toggle-crouch").Children<Toggle>().First();
            _toggleSprintToggle = RootPageElement.Query<VisualElement>("toggle-sprint").Children<Toggle>().First();
            _toggleAimToggle = RootPageElement.Query<VisualElement>("toggle-aim").Children<Toggle>().First();

            _controlsScrollView = RootPageElement.Q<ScrollView>("controls-scroll-view");

            _mainVolumeSlider = RootPageElement.Query<VisualElement>("main").Children<SliderInt>("slider").First();
            _musicVolumeSlider = RootPageElement.Query<VisualElement>("music").Children<SliderInt>("slider").First();
            _sfxVolumeSlider = RootPageElement.Query<VisualElement>("sound-effects").Children<SliderInt>("slider")
                .First();
            _dialogueVolumeSlider =
                RootPageElement.Query<VisualElement>("dialogue").Children<SliderInt>("slider").First();
            _ambientVolumeSlider =
                RootPageElement.Query<VisualElement>("ambient").Children<SliderInt>("slider").First();
            _uiVolumeSlider = RootPageElement.Query<VisualElement>("ui").Children<SliderInt>("slider").First();


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

            SetUpFocusElements();


            // Setup control buttons
            _cancelRebindButton.clicked += CancelRebind;
            _applyButton.clicked += ApplySettings;
            _resetButton.clicked += ResetToDefaults;
            _closeButton.clicked += () =>
            {
                CanExit = true;
                UIStateMachine.BackButtonClicked();
            };

            _controlsScrollView.RegisterCallback<FocusInEvent>(OnScrollViewFocusIn);
            foreach (var element in _controlsScrollView.Children())
                element.RegisterCallback<FocusInEvent>(evt => { _controlsScrollView.ScrollTo(element); });
            _inputActions.InMenuCancel += OnCancel;


            _controlsScrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            _inputActions.Navigate += OnNavigate;
        }

        private void OnNavigate(Vector2 arg0)
        {
            if (Time.time < _scrollCooldown) return;
            _scrollCooldown = Time.time + ScrollCooldownTime;

            _controlsScrollView.scrollOffset += new Vector2(0, -arg0.y * 50f); // Adjust multiplier
        }

        private void SetUpFocusElements()
        {
            RootPageElement.Query<VisualElement>(className: ".unity-text-field").ForEach(e => { e.focusable = false; });


            foreach (var visualElement in _sensitivitySlider.Children().Where(c => c.focusable))
                visualElement.focusable = false;
            var allElements = RootPageElement.Query<VisualElement>().Where(e => e.focusable).ToList();

            RootPageElement.RegisterCallback<NavigationMoveEvent>(evt =>
            {
                if (evt.direction == NavigationMoveEvent.Direction.Up)
                    _controlsScrollView.scrollOffset += new Vector2(0, -300f);
                else if (evt.direction == NavigationMoveEvent.Direction.Down)
                    _controlsScrollView.scrollOffset += new Vector2(0, 300f);
            });
            foreach (var element in allElements)
            {
                element.RegisterCallback<FocusInEvent>(evt => { element.AddToClassList("settings-element-focused"); });

                element.RegisterCallback<FocusOutEvent>(evt =>
                {
                    element.RemoveFromClassList("settings-element-focused");
                });
            }
        }

        private static bool IsFullyVisible(VisualElement element, ScrollView scrollView)
        {
            var elementBound = element.worldBound;
            var viewportBound = scrollView.contentViewport.worldBound;

            return viewportBound.Contains(elementBound.min) &&
                   viewportBound.Contains(elementBound.max);
        }

        private void OnCancel()
        {
            if (!IsActive) return;
            if (_rebindOperation != null)
            {
                CancelRebind();
            }

            else if (!_isScrollViewFocused)
            {
                CanExit = true;
            }
            else
            {
                _applyButton.focusable = true;
                _resetButton.focusable = true;
                _closeButton.focusable = true;

                _applyButton.Focus();
                _bottomButtonFocusIndex = 1;
                _isScrollViewFocused = false;
            }
        }

        private void OnScrollViewFocusIn(FocusInEvent evt)
        {
            CanExit = false;
            _applyButton.focusable = false;
            _resetButton.focusable = false;
            _closeButton.focusable = false;
            _isScrollViewFocused = true;
        }

        private void SetupCompositeBindings(string actionName)
        {
            if (!_actionMap.TryGetValue(actionName, out var reference)) return;

            if (!_compositeConfig.TryGetValue(actionName, out var parts)) return;
            reference.action.Disable();

            foreach (var (partName, uiElementName) in parts)
                SetupCompositePartBindings(actionName, partName, uiElementName);
        }

        private void SetupCompositePartBindings(string actionName, string partName, string uiElementName)
        {
            if (!_actionMap.TryGetValue(actionName, out var reference)) return;

            var partIndices = FindCompositePartIndices(reference.action, partName);
            if (partIndices.Count == 0) return;

            var keyboardButton = GetBindButtonsUnderTab(_keyboardBindsContainer, uiElementName);
            var controllerButton = GetBindButtonsUnderTab(_controllerBindsContainer, uiElementName);


            // Find the correct binding index for each control scheme among the part indices
            var keyboardBindingIndex = FindBindingIndexForScheme(reference.action, partIndices, "Keyboard&Mouse");
            var gamepadBindingIndex = FindBindingIndexForScheme(reference.action, partIndices, "Gamepad");

            var keyboardPartBinding = reference.action.bindings[keyboardBindingIndex];
            var gamepadPartBinding = reference.action.bindings[gamepadBindingIndex];


            // Update buttons with icons
            if (keyboardBindingIndex >= 0)
                UpdateButtonWithIcon(keyboardButton, keyboardPartBinding.effectivePath);

            if (gamepadBindingIndex >= 0)
                UpdateButtonWithIcon(controllerButton, gamepadPartBinding.effectivePath,
                    _currentDeviceFolder);

            // Setup click handlers
            if (keyboardButton != null && keyboardBindingIndex >= 0)
                keyboardButton.clicked += () => StartRebind(actionName, keyboardBindingIndex, keyboardButton);

            if (controllerButton != null && gamepadBindingIndex >= 0)
                controllerButton.clicked += () => StartRebind(actionName, gamepadBindingIndex, controllerButton);
        }

        private int FindBindingIndexForScheme(InputAction action, List<int> partIndices, string bindingGroup)
        {
            foreach (var index in partIndices)
            {
                var binding = action.bindings[index];
                if (binding.groups != null && binding.groups.Contains(bindingGroup))
                    return index;
            }

            return -1;
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
            if (!_actionMap.TryGetValue(actionName, out var reference)) return;

            var keyboardButton = GetBindButtonsUnderTab(_keyboardBindsContainer, actionName);
            var controllerButton = GetBindButtonsUnderTab(_controllerBindsContainer, actionName);

            // Use GetBindingIndex with group filter
            var keyboardBindingIndex = reference.action.GetBindingIndex(InputBinding.MaskByGroup("Keyboard&Mouse"));
            var gamepadBindingIndex = reference.action.GetBindingIndex(InputBinding.MaskByGroup("Gamepad"));

            if (keyboardBindingIndex >= 0)
                UpdateButtonWithIcon(keyboardButton, reference.action.bindings[keyboardBindingIndex].effectivePath);

            if (gamepadBindingIndex >= 0)
                UpdateButtonWithIcon(controllerButton, reference.action.bindings[gamepadBindingIndex].effectivePath,
                    _currentDeviceFolder);

            if (keyboardBindingIndex >= 0)
                keyboardButton.clicked += () => StartRebind(actionName, keyboardBindingIndex, keyboardButton);


            if (gamepadBindingIndex >= 0)
                controllerButton.clicked += () => StartRebind(actionName, gamepadBindingIndex, controllerButton);
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
            _logger.Log($"Rebinding complete {bindingPath}" + _inputActions.InputActions.bindings);

            // Update button with icon or text
            UpdateButtonWithIcon(button, bindingPath);


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

            if (bindingPath.ToLower().Contains("dpad"))
                _logger.Log($"Rebinding complete {bindingPath}" + _inputActions.InputActions.bindings);

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


                _logger.LogWarning($"Icon not found at path: {iconPath} for binding: {bindingPath}");
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
            GetBindButtonsUnderTab(_keyboardBindsContainer, uiElementName);

            GetBindButtonsUnderTab(_controllerBindsContainer, uiElementName);


            FindCompositePartIndices(action, partName);
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
            if (!_actionMap.TryGetValue(actionName, out var reference)) return;
            if (reference.action.bindings.Count <= 0) return;

            var keyboardButton = GetBindButtonsUnderTab(_keyboardBindsContainer, actionName);
            var controllerButton = GetBindButtonsUnderTab(_controllerBindsContainer, actionName);

            var keyboardBindingIndex = reference.action.GetBindingIndex(InputBinding.MaskByGroup("Keyboard&Mouse"));
            var gamepadBindingIndex = reference.action.GetBindingIndex(InputBinding.MaskByGroup("Gamepad"));

            if (keyboardButton != null && keyboardBindingIndex >= 0)
                UpdateButtonWithIcon(keyboardButton, reference.action.bindings[keyboardBindingIndex].effectivePath);

            if (controllerButton != null && gamepadBindingIndex >= 0)
                UpdateButtonWithIcon(controllerButton, reference.action.bindings[gamepadBindingIndex].effectivePath,
                    _currentDeviceFolder);
        }

        private static Button GetBindButtonsUnderTab(VisualElement t, string actionName)
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

            UpdateAllButtonTexts();
            _logger.Log("Reset to defaults!");
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