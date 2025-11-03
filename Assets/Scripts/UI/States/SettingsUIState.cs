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
        private readonly Dictionary<string, InputActionReference> _actionMap = new();

        // Current device type detection
        private string _currentDeviceFolder = "Keyboard & Mouse";

        // Icon mapping for keyboard and controller inputs
        private readonly Dictionary<string, string> _keyIconMap = new()
        {
            // Keyboard - Letters
            { "<Keyboard>/a", "A_Key_Light" },
            { "<Keyboard>/b", "B_Key_Light" },
            { "<Keyboard>/c", "C_Key_Light" },
            { "<Keyboard>/d", "D_Key_Light" },
            { "<Keyboard>/e", "E_Key_Light" },
            { "<Keyboard>/f", "F_Key_Light" },
            { "<Keyboard>/g", "G_Key_Light" },
            { "<Keyboard>/h", "H_Key_Light" },
            { "<Keyboard>/i", "I_Key_Light" },
            { "<Keyboard>/j", "J_Key_Light" },
            { "<Keyboard>/k", "K_Key_Light" },
            { "<Keyboard>/l", "L_Key_Light" },
            { "<Keyboard>/m", "M_Key_Light" },
            { "<Keyboard>/n", "N_Key_Light" },
            { "<Keyboard>/o", "O_Key_Light" },
            { "<Keyboard>/p", "P_Key_Light" },
            { "<Keyboard>/q", "Q_Key_Light" },
            { "<Keyboard>/r", "R_Key_Light" },
            { "<Keyboard>/s", "S_Key_Light" },
            { "<Keyboard>/t", "T_Key_Light" },
            { "<Keyboard>/u", "U_Key_Light" },
            { "<Keyboard>/v", "V_Key_Light" },
            { "<Keyboard>/w", "W_Key_Light" },
            { "<Keyboard>/x", "X_Key_Light" },
            { "<Keyboard>/y", "Y_Key_Light" },
            { "<Keyboard>/z", "Z_Key_Light" },
            
            // Keyboard - Numbers
            { "<Keyboard>/1", "1_Key_Light" },
            { "<Keyboard>/2", "2_Key_Light" },
            { "<Keyboard>/3", "3_Key_Light" },
            { "<Keyboard>/4", "4_Key_Light" },
            { "<Keyboard>/5", "5_Key_Light" },
            { "<Keyboard>/6", "6_Key_Light" },
            { "<Keyboard>/7", "7_Key_Light" },
            { "<Keyboard>/8", "8_Key_Light" },
            { "<Keyboard>/9", "9_Key_Light" },
            { "<Keyboard>/0", "0_Key_Light" },
            
            // Keyboard - Special Keys
            { "<Keyboard>/space", "Space_Key_Light" },
            { "<Keyboard>/leftShift", "Shift_Key_Light" },
            { "<Keyboard>/rightShift", "Shift_Key_Light" },
            { "<Keyboard>/leftCtrl", "Ctrl_Key_Light" },
            { "<Keyboard>/rightCtrl", "Ctrl_Key_Light" },
            { "<Keyboard>/leftAlt", "Alt_Key_Light" },
            { "<Keyboard>/rightAlt", "Alt_Key_Light" },
            { "<Keyboard>/escape", "Esc_Key_Light" },
            { "<Keyboard>/enter", "Enter_Key_Light" },
            { "<Keyboard>/tab", "Tab_Key_Light" },
            { "<Keyboard>/backspace", "Backspace_Key_Light" },
            
            // Keyboard - Arrows
            { "<Keyboard>/upArrow", "Arrow_Up_Key_Light" },
            { "<Keyboard>/downArrow", "Arrow_Down_Key_Light" },
            { "<Keyboard>/leftArrow", "Arrow_Left_Key_Light" },
            { "<Keyboard>/rightArrow", "Arrow_Right_Key_Light" },
            
            // Mouse
            { "<Mouse>/leftButton", "Left_Click_Light" },
            { "<Mouse>/rightButton", "Right_Click_Light" },
            { "<Mouse>/middleButton", "Middle_Click_Light" },
            
            // Gamepad - Face Buttons (Generic mapping)
            { "<Gamepad>/buttonSouth", "Button_South" },
            { "<Gamepad>/buttonEast", "Button_East" },
            { "<Gamepad>/buttonWest", "Button_West" },
            { "<Gamepad>/buttonNorth", "Button_North" },
            
            // Gamepad - Shoulders
            { "<Gamepad>/leftShoulder", "Left_Bumper" },
            { "<Gamepad>/rightShoulder", "Right_Bumper" },
            { "<Gamepad>/leftTrigger", "Left_Trigger" },
            { "<Gamepad>/rightTrigger", "Right_Trigger" },
            
            // Gamepad - Sticks
            { "<Gamepad>/leftStick", "LS_Button_Light" },
            { "<Gamepad>/rightStick", "RS_Button_Light" },
            { "<Gamepad>/leftStickPress", "LS_Click_Light" },
            { "<Gamepad>/rightStickPress", "RS_Click_Light" },
            
            // Gamepad - D-Pad
            { "<Gamepad>/dpad/up", "DPad_Up" },
            { "<Gamepad>/dpad/down", "DPad_Down" },
            { "<Gamepad>/dpad/left", "DPad_Left" },
            { "<Gamepad>/dpad/right", "DPad_Right" },
            
            // Gamepad - Menu Buttons
            { "<Gamepad>/start", "Start_Button_Light" },
            { "<Gamepad>/select", "Select_Button_Light" }
        };

        // Settings storage
        private readonly Dictionary<string, (string primary, string secondary)> _keyBindings = new();
        private SliderInt _ambientVolumeSlider;

        private Button _applyButton;
        private Button _cancelRebindButton;
        private Button _closeButton;
        private string _currentActionName;
        private AudioSettingsChangedEvent _currentAudioSettings;
        private int _currentBindingIndex;

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
            InitializeInputSystem();
            CacheUIElements();
            SetupEventListeners();
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

        private string GetDeviceFolderFromPath(string bindingPath)
        {
            if (string.IsNullOrEmpty(bindingPath)) return "Keyboard & Mouse";

            // Detect device type from binding path
            if (bindingPath.Contains("<Keyboard>") || bindingPath.Contains("<Mouse>"))
                return "Keyboard & Mouse";
            
            if (bindingPath.Contains("<DualShockGamepad>") || bindingPath.Contains("<PS"))
                return "PS5";
            
            if (bindingPath.Contains("<XInputController>") || bindingPath.Contains("<XboxOne"))
                return "Xbox Series";
            
            if (bindingPath.Contains("<SwitchPro"))
                return "Switch";
            
            // Check for Steam Deck specific device
            if (bindingPath.Contains("<SteamDeck>"))
                return "Steam Deck";
            
            // Default to Xbox for generic gamepad
            if (bindingPath.Contains("<Gamepad>"))
                return "Xbox Series";

            return "Keyboard & Mouse";
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
            if (!_actionMap.TryGetValue(actionName, out var reference)) return;
            reference.action.Disable();

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
                primaryButton.clicked += () => StartCompositeRebind(actionName, partIndices[0], primaryButton);

            // Secondary button gets the second binding (if it exists)
            if (partIndices.Count > 1 && secondaryButton != null)
                secondaryButton.clicked += () => StartCompositeRebind(actionName, partIndices[1], secondaryButton);
        }

        private List<int> FindCompositePartIndices(InputAction action, string partName)
        {
            var indices = new List<int>();

            // Find all composite bindings first
            for (var i = 0; i < action.bindings.Count; i++)
            {
                if (!action.bindings[i].isComposite) continue;
                // Search for all parts with the matching name after this composite
                for (var j = i + 1; j < action.bindings.Count; j++)
                {
                    var binding = action.bindings[j];

                    // Stop if we hit another composite or a non-part binding
                    if (binding.isComposite || !binding.isPartOfComposite)
                        break;

                    if (binding.name.Equals(partName, StringComparison.OrdinalIgnoreCase)) indices.Add(j);
                }
            }

            return indices;
        }

        private void StartCompositeRebind(string actionName, int bindingIndex, Button button)
        {
            if (!_actionMap.TryGetValue(actionName, out var action)) return;

            // Disable the action while rebinding
            action.action.Disable();

            // Show overlay
            if (_inputOverlay != null)
            {
                _inputOverlay.style.display = DisplayStyle.Flex;
                var bindingName = action.action.bindings[bindingIndex].name;
                _waitingText.text = $"Press a key for {bindingName}...";
            }

            _currentActionName = actionName;
            _currentBindingIndex = bindingIndex;

            // Start the rebinding operation for the specific composite part
            _rebindOperation = action.action.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough("<Keyboard>/escape")
                .WithCancelingThrough("<Gamepad>/buttonEast")
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

            primaryButton.clicked += () => StartRebind(actionName, 0, primaryButton);
            secondaryButton.clicked += () => StartRebind(actionName, 1, secondaryButton);
        }

        private void StartRebind(string actionName, int bindingIndex, Button button)
        {
            if (!_actionMap.TryGetValue(actionName, out var reference)) return;
            // Disable the action while rebinding
            reference.action.Disable();

            // Show overlay
            if (_inputOverlay != null)
            {
                _inputOverlay.style.display = DisplayStyle.Flex;
                _waitingText.text = $"Press a key for {actionName.Replace("-", " ")}...";
            }

            _currentActionName = actionName;
            _currentBindingIndex = bindingIndex;

            // Start the rebinding operation
            _rebindOperation = reference.action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("Mouse")
                .WithCancelingThrough("<Keyboard>/escape")
                .WithCancelingThrough("<Gamepad>/buttonEast")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation => OnRebindComplete(button))
                .OnCancel(operation => OnRebindCancelled())
                .Start();
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
            if (!_keyBindings.ContainsKey(_currentActionName)) _keyBindings[_currentActionName] = (null, null);

            var current = _keyBindings[_currentActionName];
            if (_currentBindingIndex == 0)
                _keyBindings[_currentActionName] = (bindingPath, current.secondary);
            else
                _keyBindings[_currentActionName] = (current.primary, bindingPath);

            reference.action.Enable();
            CleanupRebind();
        }

        private void UpdateButtonWithIcon(Button button, string bindingPath)
        {
            // Clear existing content
            button.Clear();
            button.text = "";
            
            if (string.IsNullOrEmpty(bindingPath))
            {
                button.text = "None";
                return;
            }

            // Determine device folder based on binding path
            string deviceFolder = GetDeviceFolderFromPath(bindingPath);

            // Try to load icon from Resources
            if (_keyIconMap.TryGetValue(bindingPath, out var iconName))
            {
                // Build full path: UI/Icons/Light/{DeviceFolder}/{IconName}
                string iconPath = $"UI/Icons/Light/{deviceFolder}/{iconName}";
                
                var texture = Resources.LoadAsync<Texture2D>(iconPath);
                texture.completed += _ =>
                {
                    if (texture.asset != null)
                    {
                        var image = new Image
                        {
                            image = texture.asset as Texture2D,
                            scaleMode = ScaleMode.ScaleToFit
                        };
                        image.AddToClassList("key-icon");
                        button.Add(image);
                        return;
                    }

                    Debug.LogWarning($"Icon not found at path: {iconPath} for binding: {bindingPath}");
                    // Fallback to text if icon not found
                    button.text = GetBindingDisplayString(bindingPath);
                };
            }
            else
            {
                // Fallback to text if no icon mapping found
                button.text = GetBindingDisplayString(bindingPath);
            }
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
                UpdateButtonWithIcon(primaryButton, action.bindings[partIndices[0]].effectivePath);

            // Update secondary button with second binding
            if (partIndices.Count > 1 && secondaryButton != null)
                UpdateButtonWithIcon(secondaryButton, action.bindings[partIndices[1]].effectivePath);
            else if (secondaryButton != null)
            {
                secondaryButton.Clear();
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
            if (_actionMap.TryGetValue(_currentActionName, out var value)) value.action.Enable();
        }

        private void UpdateAllButtonTexts()
        {
            foreach (var kvp in _actionMap)
                if (kvp.Key == "move" || kvp.Key == "lean")
                    UpdateCompositeButtonTexts(kvp.Key);
                else
                    UpdateButtonText(kvp.Key);
        }

        private void UpdateButtonText(string actionName)
        {
            var container = RootPageElement.Q<VisualElement>(actionName);
            if (container == null) return;

            var buttonsContainer = container.Q<VisualElement>("buttons");
            if (buttonsContainer == null) return;

            var primaryButton = buttonsContainer.Q<Button>("primary");
            var secondaryButton = buttonsContainer.Q<Button>("secondary");

            var reference = _actionMap[actionName];
            if (reference.action.bindings.Count <= 0) return;
            
            if (primaryButton != null)
                UpdateButtonWithIcon(primaryButton, reference.action.bindings[0].effectivePath);

            if (reference.action.bindings.Count > 1 && secondaryButton != null)
                UpdateButtonWithIcon(secondaryButton, reference.action.bindings[1].effectivePath);
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
            public Dictionary<string, (string primary, string secondary)> Bindings;
        }
    }
}