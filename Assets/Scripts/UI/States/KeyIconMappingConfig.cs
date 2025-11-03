using System;
using System.Collections.Generic;

namespace UI.States
{
    public sealed class KeyIconMappingConfig
    {
  
        private List<KeyIconMapping> keyIconMappings = new()
        {
            // Keyboard - Letters
            new("<Keyboard>/a", "A_Key_Light"),
            new("<Keyboard>/b", "B_Key_Light"),
            new("<Keyboard>/c", "C_Key_Light"),
            new("<Keyboard>/d", "D_Key_Light"),
            new("<Keyboard>/e", "E_Key_Light"),
            new("<Keyboard>/f", "F_Key_Light"),
            new("<Keyboard>/g", "G_Key_Light"),
            new("<Keyboard>/h", "H_Key_Light"),
            new("<Keyboard>/i", "I_Key_Light"),
            new("<Keyboard>/j", "J_Key_Light"),
            new("<Keyboard>/k", "K_Key_Light"),
            new("<Keyboard>/l", "L_Key_Light"),
            new("<Keyboard>/m", "M_Key_Light"),
            new("<Keyboard>/n", "N_Key_Light"),
            new("<Keyboard>/o", "O_Key_Light"),
            new("<Keyboard>/p", "P_Key_Light"),
            new("<Keyboard>/q", "Q_Key_Light"),
            new("<Keyboard>/r", "R_Key_Light"),
            new("<Keyboard>/s", "S_Key_Light"),
            new("<Keyboard>/t", "T_Key_Light"),
            new("<Keyboard>/u", "U_Key_Light"),
            new("<Keyboard>/v", "V_Key_Light"),
            new("<Keyboard>/w", "W_Key_Light"),
            new("<Keyboard>/x", "X_Key_Light"),
            new("<Keyboard>/y", "Y_Key_Light"),
            new("<Keyboard>/z", "Z_Key_Light"),
        
            // Keyboard - Numbers
            new("<Keyboard>/1", "1_Key_Light"),
            new("<Keyboard>/2", "2_Key_Light"),
            new("<Keyboard>/3", "3_Key_Light"),
            new("<Keyboard>/4", "4_Key_Light"),
            new("<Keyboard>/5", "5_Key_Light"),
            new("<Keyboard>/6", "6_Key_Light"),
            new("<Keyboard>/7", "7_Key_Light"),
            new("<Keyboard>/8", "8_Key_Light"),
            new("<Keyboard>/9", "9_Key_Light"),
            new("<Keyboard>/0", "0_Key_Light"),
        
            // Keyboard - Special Keys
            new("<Keyboard>/space", "Space_Key_Light"),
            new("<Keyboard>/leftShift", "Shift_Key_Light"),
            new("<Keyboard>/rightShift", "Shift_Key_Light"),
            new("<Keyboard>/leftCtrl", "Ctrl_Key_Light"),
            new("<Keyboard>/rightCtrl", "Ctrl_Key_Light"),
            new("<Keyboard>/leftAlt", "Alt_Key_Light"),
            new("<Keyboard>/rightAlt", "Alt_Key_Light"),
            new("<Keyboard>/escape", "Esc_Key_Light"),
            new("<Keyboard>/enter", "Enter_Key_Light"),
            new("<Keyboard>/tab", "Tab_Key_Light"),
            new("<Keyboard>/backspace", "Backspace_Key_Light"),
        
            // Keyboard - Arrows
            new("<Keyboard>/upArrow", "Arrow_Up_Key_Light"),
            new("<Keyboard>/downArrow", "Arrow_Down_Key_Light"),
            new("<Keyboard>/leftArrow", "Arrow_Left_Key_Light"),
            new("<Keyboard>/rightArrow", "Arrow_Right_Key_Light"),
        
            // Mouse
            new("<Mouse>/leftButton", "Left_Click_Light"),
            new("<Mouse>/rightButton", "Right_Click_Light"),
            new("<Mouse>/middleButton", "Middle_Click_Light"),
        
            // Gamepad - Face Buttons
            new("<Gamepad>/buttonSouth", "Button_South"),
            new("<Gamepad>/buttonEast", "Button_East"),
            new("<Gamepad>/buttonWest", "Button_West"),
            new("<Gamepad>/buttonNorth", "Button_North"),
        
            // Gamepad - Shoulders
            new("<Gamepad>/leftShoulder", "Left_Bumper"),
            new("<Gamepad>/rightShoulder", "Right_Bumper"),
            new("<Gamepad>/leftTrigger", "Left_Trigger"),
            new("<Gamepad>/rightTrigger", "Right_Trigger"),
        
            // Gamepad - Sticks
            new("<Gamepad>/leftStick", "Left_Stick"),
            new("<Gamepad>/rightStick", "Right_Stick"),
            new("<Gamepad>/leftStickPress", "Left_Stick_Click"),
            new("<Gamepad>/rightStickPress", "Right_Stick_Click"),
        
            // Gamepad - D-Pad
            new("<Gamepad>/dpad/up", "DPad_Up"),
            new("<Gamepad>/dpad/down", "DPad_Down"),
            new("<Gamepad>/dpad/left", "DPad_Left"),
            new("<Gamepad>/dpad/right", "DPad_Right"),
        
            // Gamepad - Menu Buttons
            new("<Gamepad>/start", "Start"),
            new("<Gamepad>/select", "Select")
        };
    
        // Cached dictionary for runtime lookups
        private Dictionary<string, string> _keyIconMap;

        public Dictionary<string, string> KeyIconMap
        {
            get
            {
                if (_keyIconMap == null)
                {
                    BuildDictionary();
                }

                return _keyIconMap;
            }
        }
    
        /// <summary>
        /// Gets the icon name for a given input path
        /// </summary>
        public string GetIconName(string inputPath)
        {
            if (_keyIconMap == null)
                BuildDictionary();
            
            return KeyIconMap.GetValueOrDefault(inputPath, String.Empty);
        }
    

        /// <summary>
        /// Rebuilds the internal dictionary (call if you modify mappings at runtime)
        /// </summary>
        public void BuildDictionary()
        {
            _keyIconMap = new Dictionary<string, string>();
            foreach (var mapping in keyIconMappings)
            {
                if (!string.IsNullOrEmpty(mapping.inputPath))
                {
                    _keyIconMap[mapping.inputPath] = mapping.iconName;
                }
            }
        }
    
        private void OnEnable()
        {
            BuildDictionary();
        }
    }
}