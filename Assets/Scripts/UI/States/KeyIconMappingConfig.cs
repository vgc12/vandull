using System.Collections.Generic;

namespace UI.States
{
    public sealed class KeyIconMappingConfig
    {
        private readonly List<KeyIconMapping> keyIconMappings = new()
        {
            // Keyboard - Letters
            new KeyIconMapping("<Keyboard>/a", "A_Key_Dark"),
            new KeyIconMapping("<Keyboard>/b", "B_Key_Dark"),
            new KeyIconMapping("<Keyboard>/c", "C_Key_Dark"),
            new KeyIconMapping("<Keyboard>/d", "D_Key_Dark"),
            new KeyIconMapping("<Keyboard>/e", "E_Key_Dark"),
            new KeyIconMapping("<Keyboard>/f", "F_Key_Dark"),
            new KeyIconMapping("<Keyboard>/g", "G_Key_Dark"),
            new KeyIconMapping("<Keyboard>/h", "H_Key_Dark"),
            new KeyIconMapping("<Keyboard>/i", "I_Key_Dark"),
            new KeyIconMapping("<Keyboard>/j", "J_Key_Dark"),
            new KeyIconMapping("<Keyboard>/k", "K_Key_Dark"),
            new KeyIconMapping("<Keyboard>/l", "L_Key_Dark"),
            new KeyIconMapping("<Keyboard>/m", "M_Key_Dark"),
            new KeyIconMapping("<Keyboard>/n", "N_Key_Dark"),
            new KeyIconMapping("<Keyboard>/o", "O_Key_Dark"),
            new KeyIconMapping("<Keyboard>/p", "P_Key_Dark"),
            new KeyIconMapping("<Keyboard>/q", "Q_Key_Dark"),
            new KeyIconMapping("<Keyboard>/r", "R_Key_Dark"),
            new KeyIconMapping("<Keyboard>/s", "S_Key_Dark"),
            new KeyIconMapping("<Keyboard>/t", "T_Key_Dark"),
            new KeyIconMapping("<Keyboard>/u", "U_Key_Dark"),
            new KeyIconMapping("<Keyboard>/v", "V_Key_Dark"),
            new KeyIconMapping("<Keyboard>/w", "W_Key_Dark"),
            new KeyIconMapping("<Keyboard>/x", "X_Key_Dark"),
            new KeyIconMapping("<Keyboard>/y", "Y_Key_Dark"),
            new KeyIconMapping("<Keyboard>/z", "Z_Key_Dark"),

            // Keyboard - Numbers
            new KeyIconMapping("<Keyboard>/1", "1_Key_Dark"),
            new KeyIconMapping("<Keyboard>/2", "2_Key_Dark"),
            new KeyIconMapping("<Keyboard>/3", "3_Key_Dark"),
            new KeyIconMapping("<Keyboard>/4", "4_Key_Dark"),
            new KeyIconMapping("<Keyboard>/5", "5_Key_Dark"),
            new KeyIconMapping("<Keyboard>/6", "6_Key_Dark"),
            new KeyIconMapping("<Keyboard>/7", "7_Key_Dark"),
            new KeyIconMapping("<Keyboard>/8", "8_Key_Dark"),
            new KeyIconMapping("<Keyboard>/9", "9_Key_Dark"),
            new KeyIconMapping("<Keyboard>/0", "0_Key_Dark"),

            // Keyboard - Special Keys
            new KeyIconMapping("<Keyboard>/space", "Space_Key_Dark"),
            new KeyIconMapping("<Keyboard>/leftShift", "Shift_Key_Dark"),
            new KeyIconMapping("<Keyboard>/rightShift", "Shift_Key_Dark"),
            new KeyIconMapping("<Keyboard>/ctrl", "Ctrl_Key_Dark"),
            new KeyIconMapping("<Keyboard>/ctrl", "Ctrl_Key_Dark"),
            new KeyIconMapping("<Keyboard>/leftAlt", "Alt_Key_Dark"),
            new KeyIconMapping("<Keyboard>/rightAlt", "Alt_Key_Dark"),
            new KeyIconMapping("<Keyboard>/escape", "Esc_Key_Dark"),
            new KeyIconMapping("<Keyboard>/enter", "Enter_Key_Dark"),
            new KeyIconMapping("<Keyboard>/tab", "Tab_Key_Dark"),
            new KeyIconMapping("<Keyboard>/backspace", "Backspace_Key_Dark"),

            // Keyboard - Arrows
            new KeyIconMapping("<Keyboard>/upArrow", "Arrow_Up_Key_Dark"),
            new KeyIconMapping("<Keyboard>/downArrow", "Arrow_Down_Key_Dark"),
            new KeyIconMapping("<Keyboard>/leftArrow", "Arrow_Left_Key_Dark"),
            new KeyIconMapping("<Keyboard>/rightArrow", "Arrow_Right_Key_Dark"),

            // Mouse
            new KeyIconMapping("<Mouse>/leftButton", "Mouse_Left_Key_Dark"),
            new KeyIconMapping("<Mouse>/rightButton", "Mouse_Right_Key_Dark"),
            new KeyIconMapping("<Mouse>/middleButton", "Mouse_Middle_Key_Dark"),
            new KeyIconMapping("<Mouse>/scroll/up", "Mouse_Middle_Key_Dark"),
            new KeyIconMapping("<Mouse>/scroll/down", "Mouse_Middle_Key_Dark"),

            // Gamepad - Face Buttons
            new KeyIconMapping("<Gamepad>/buttonSouth", "Button_South"),
            new KeyIconMapping("<Gamepad>/buttonEast", "Button_East"),
            new KeyIconMapping("<Gamepad>/buttonWest", "Button_West"),
            new KeyIconMapping("<Gamepad>/buttonNorth", "Button_North"),

            // Gamepad - Shoulders
            new KeyIconMapping("<Gamepad>/leftShoulder", "Left_Bumper"),
            new KeyIconMapping("<Gamepad>/rightShoulder", "Right_Bumper"),
            new KeyIconMapping("<Gamepad>/leftTrigger", "Left_Trigger"),
            new KeyIconMapping("<Gamepad>/rightTrigger", "Right_Trigger"),

            // Gamepad - Sticks
            new KeyIconMapping("<Gamepad>/leftStick", "Left_Stick"),
            new KeyIconMapping("<Gamepad>/rightStick", "Right_Stick"),
            new KeyIconMapping("<Gamepad>/leftStickPress", "Left_Stick_Click"),
            new KeyIconMapping("<Gamepad>/rightStickPress", "Right_Stick_Click"),

            // Gamepad - D-Pad
            new KeyIconMapping("<Gamepad>/dpad/up", "DPad_Up"),
            new KeyIconMapping("<Gamepad>/dpad/down", "DPad_Down"),
            new KeyIconMapping("<Gamepad>/dpad/left", "DPad_Left"),
            new KeyIconMapping("<Gamepad>/dpad/right", "DPad_Right"),

            // Gamepad - Menu Buttons
            new KeyIconMapping("<Gamepad>/start", "Start"),
            new KeyIconMapping("<Gamepad>/select", "Select")
        };

        // Cached dictionary for runtime lookups
        private Dictionary<string, string> _keyIconMap;

        public Dictionary<string, string> KeyIconMap
        {
            get
            {
                if (_keyIconMap == null) BuildDictionary();

                return _keyIconMap;
            }
        }

        /// <summary>
        ///     Gets the icon name for a given input path
        /// </summary>
        public string GetIconName(string inputPath)
        {
            if (_keyIconMap == null)
                BuildDictionary();

            return KeyIconMap.GetValueOrDefault(inputPath, string.Empty);
        }


        /// <summary>
        ///     Rebuilds the internal dictionary (call if you modify mappings at runtime)
        /// </summary>
        public void BuildDictionary()
        {
            _keyIconMap = new Dictionary<string, string>();
            foreach (var mapping in keyIconMappings)
                if (!string.IsNullOrEmpty(mapping.inputPath))
                    _keyIconMap[mapping.inputPath] = mapping.iconName;
        }

        private void OnEnable()
        {
            BuildDictionary();
        }
    }
}