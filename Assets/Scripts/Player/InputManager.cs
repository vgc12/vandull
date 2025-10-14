using UnityEngine;

namespace Player
{
    public class InputManager : MonoBehaviour
    {
        public PlayerInputActions InputActions { get; private set; }

        public static InputManager Instance { get; private set; }
        
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
      
            Instance = this;
            InputActions = new PlayerInputActions();
            InputActions.Player.Enable();
        
        }


        private void OnDestroy()
        {
            InputActions.Player.Disable();
        }
        
        
        
    }
}