using UnityEngine;

namespace Player
{
    public class InputManager : MonoBehaviour
    {
        public PlayerInputActions InputActions { get; private set; }

 
        private void Awake()
        {
            InputActions = new PlayerInputActions();
            InputActions.Player.Enable();
            
     
        }

    }
}
